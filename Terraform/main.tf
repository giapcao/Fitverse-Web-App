locals {
  service_connect_domain = var.enable_service_connect ? "${var.project_name}.${var.service_discovery_domain_suffix}" : null

  service_env_var_maps = {
    for svc_name, svc in var.services :
    svc_name => {
      for env_var in try(svc.ecs_environment_variables, []) :
      env_var.name => env_var.value
    }
  }

  service_dns_names = {
    for svc_name, svc in var.services :
    svc_name => (
      local.service_connect_domain != null && try(svc.ecs_service_connect_dns_name, "") != "" ?
      "${svc.ecs_service_connect_dns_name}.${local.service_connect_domain}" :
      try(svc.ecs_service_connect_dns_name, svc_name)
    )
  }

  service_primary_ports = {
    for svc_name, svc in var.services :
    svc_name => try(svc.ecs_container_port_mappings[0].container_port, null)
  }

  rabbitmq_dns  = lookup(local.service_dns_names, "rabbitmq", null)
  rabbitmq_port = lookup(local.service_primary_ports, "rabbitmq", 5672)
  rabbitmq_env  = lookup(local.service_env_var_maps, "rabbitmq", {})
  rabbitmq_user = lookup(rabbitmq_env, "RABBITMQ_DEFAULT_USER", "")
  rabbitmq_pass = lookup(rabbitmq_env, "RABBITMQ_DEFAULT_PASS", "")

  redis_dns      = lookup(local.service_dns_names, "redis", null)
  redis_port     = lookup(local.service_primary_ports, "redis", 6379)
  redis_env      = lookup(local.service_env_var_maps, "redis", {})
  redis_password = lookup(redis_env, "REDIS_PASSWORD", "")

  rabbitmq_dependency_defaults = rabbitmq_dns != null ? merge(
    {
      RABBITMQ_HOST = rabbitmq_dns
      RABBITMQ_PORT = tostring(rabbitmq_port)
    },
    rabbitmq_user != "" ? { RABBITMQ_USERNAME = rabbitmq_user } : {},
    rabbitmq_pass != "" ? { RABBITMQ_PASSWORD = rabbitmq_pass } : {},
    rabbitmq_user != "" && rabbitmq_pass != "" ? {
      RABBITMQ_URL = format(
        "amqp://%s:%s@%s:%s/",
        rabbitmq_user,
        rabbitmq_pass,
        rabbitmq_dns,
        rabbitmq_port
      )
    } : {}
  ) : {}

  redis_dependency_defaults = redis_dns != null ? merge(
    {
      REDIS_HOST = redis_dns
      REDIS_PORT = tostring(redis_port)
    },
    redis_password != "" ? { REDIS_PASSWORD = redis_password } : {}
  ) : {}

  service_dependency_defaults = {
    for svc_name in ["authentication", "coach", "payment", "booking"] :
    svc_name => merge(local.rabbitmq_dependency_defaults, local.redis_dependency_defaults)
  }

  merged_service_env_vars = {
    for svc_name, env_map in local.service_env_var_maps :
    svc_name => [
      for key, value in merge(
        lookup(local.service_dependency_defaults, svc_name, {}),
        env_map
        ) : {
        name  = key
        value = value
      }
    ]
  }
}

# VPC Module
module "vpc" {
  source              = "./modules/vpc"
  project_name        = var.project_name
  vpc_cidr            = var.vpc_cidr
  public_subnet_cidrs = var.public_subnet_cidrs
  public_subnet_count = 2
  private_subnet_cidr = var.private_subnet_cidr
}

# ALB Module
module "alb" {
  source            = "./modules/alb"
  project_name      = var.project_name
  vpc_id            = module.vpc.vpc_id
  public_subnet_ids = module.vpc.public_subnet_ids

  target_groups_definition = [
    {
      # API Gateway Target Group
      name_suffix = "apigateway"
      port        = var.services["apigateway"].alb_target_group_port
      protocol    = var.services["apigateway"].alb_target_group_protocol
      target_type = var.services["apigateway"].alb_target_group_type
      health_check = {
        enabled             = true
        path                = "/api/health"
        port                = var.services["apigateway"].alb_health_check.port
        protocol            = var.services["apigateway"].alb_health_check.protocol
        matcher             = var.services["apigateway"].alb_health_check.matcher
        interval            = var.services["apigateway"].alb_health_check.interval
        timeout             = var.services["apigateway"].alb_health_check.timeout
        healthy_threshold   = var.services["apigateway"].alb_health_check.healthy_threshold
        unhealthy_threshold = var.services["apigateway"].alb_health_check.unhealthy_threshold
      }
    }
  ]

  default_listener_action = {
    type                = "forward"
    target_group_suffix = "apigateway"
  }

  listener_rules_definition = []
}

# EC2 Module
module "ec2" {
  source                = "./modules/ec2"
  project_name          = var.project_name
  vpc_id                = module.vpc.vpc_id
  vpc_cidr              = var.vpc_cidr
  subnet_id             = module.vpc.private_subnet_id
  instance_type         = var.instance_type
  associate_public_ip   = var.associate_public_ip
  alb_security_group_id = module.alb.alb_sg_id
  container_instance_groups = {
    server-1 = {
      instance_attributes = { service_group = "server-1" }
      tags                = { ServiceGroup = "server-1" }
      user_data_extra     = <<-EOF
        mkdir -p /var/lib/${var.project_name}/rabbitmq
        mkdir -p /var/lib/${var.project_name}/redis
        chown 999:999 /var/lib/${var.project_name}/rabbitmq || true
        chown 999:999 /var/lib/${var.project_name}/redis || true
        chmod 0775 /var/lib/${var.project_name}/rabbitmq || chmod 0777 /var/lib/${var.project_name}/rabbitmq
        chmod 0775 /var/lib/${var.project_name}/redis || chmod 0777 /var/lib/${var.project_name}/redis
      EOF
    }
    server-2 = {
      instance_attributes = { service_group = "server-2" }
      tags                = { ServiceGroup = "server-2" }
      user_data_extra     = ""
    }
  }

  depends_on = [module.alb]
}

# Shared ECS Resources (created once, used by all services)
resource "aws_cloudwatch_log_group" "ecs_logs" {
  name              = "/ecs/${var.project_name}"
  retention_in_days = 30
  tags              = { Name = "${var.project_name}-ecs-logs" }
}

resource "aws_iam_role" "ecs_task_role" {
  name = "${var.project_name}-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "ecs-tasks.amazonaws.com" }
    }]
  })
  tags = { Name = "${var.project_name}-ecs-task-role" }
}

resource "aws_iam_role" "ecs_execution_role" {
  name = "${var.project_name}-ecs-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "ecs-tasks.amazonaws.com" }
    }]
  })
  tags = { Name = "${var.project_name}-ecs-execution-role" }
}

resource "aws_iam_role_policy" "ecs_task_policy" {
  name = "${var.project_name}-ecs-task-policy"
  role = aws_iam_role.ecs_task_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["logs:CreateLogStream", "logs:PutLogEvents"]
      Resource = "${aws_cloudwatch_log_group.ecs_logs.arn}:*"
    }]
  })
}

resource "aws_iam_role_policy_attachment" "ecs_execution_managed" {
  role       = aws_iam_role.ecs_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy_attachment" "ecs_task_ecr_pull" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
}

resource "aws_security_group" "ecs_task_sg" {
  name_prefix = "${var.project_name}-ecs-task-sg-"
  description = "Security group for ECS tasks (awsvpc)"
  vpc_id      = module.vpc.vpc_id

  ingress {
    description     = "Allow inbound from ALB"
    from_port       = 0
    to_port         = 65535
    protocol        = "tcp"
    security_groups = [module.alb.alb_sg_id]
  }

  ingress {
    description = "Allow intra-VPC task-to-task"
    from_port   = 0
    to_port     = 65535
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
  }

  egress {
    description = "All outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "${var.project_name}-ecs-task-sg" }
}

resource "aws_security_group_rule" "task_sg_intra_self" {
  type              = "ingress"
  description       = "Allow all traffic within ECS task SG"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  security_group_id = aws_security_group.ecs_task_sg.id
  self              = true
}

resource "aws_service_discovery_private_dns_namespace" "ecs_namespace" {
  count       = var.enable_service_connect ? 1 : 0
  name        = "${var.project_name}.${var.service_discovery_domain_suffix}"
  vpc         = module.vpc.vpc_id
  description = "Service discovery namespace for ${var.project_name}"
  tags        = { Name = "${var.project_name}-dns-namespace" }
}

# ECS Module - Server-1 (Authentication + Coach microservices with RabbitMQ + Redis)
module "ecs_server1" {
  source = "./modules/ecs"

  project_name             = var.project_name
  aws_region               = var.aws_region
  vpc_id                   = module.vpc.vpc_id
  vpc_cidr                 = var.vpc_cidr
  task_subnet_ids          = [module.vpc.private_subnet_id]
  ecs_cluster_id           = module.ec2.ecs_cluster_arn
  ecs_cluster_name         = module.ec2.ecs_cluster_name
  alb_security_group_id    = module.alb.alb_sg_id
  assign_public_ip         = false
  desired_count            = 1
  service_names            = ["server-1"]
  service_discovery_domain = "${var.project_name}.${var.service_discovery_domain_suffix}"
  service_dependencies     = {}
  enable_auto_scaling      = var.enable_auto_scaling
  enable_service_connect   = var.enable_service_connect

  # Pass shared resources
  shared_log_group_name     = aws_cloudwatch_log_group.ecs_logs.name
  shared_task_role_arn      = aws_iam_role.ecs_task_role.arn
  shared_execution_role_arn = aws_iam_role.ecs_execution_role.arn
  shared_task_sg_id         = aws_security_group.ecs_task_sg.id
  service_connect_namespace = var.enable_service_connect ? aws_service_discovery_private_dns_namespace.ecs_namespace[0].arn : null

  service_connect_services = {
    server-1 = [
      {
        port_name      = var.services["authentication"].ecs_service_connect_port_name
        discovery_name = var.services["authentication"].ecs_service_connect_discovery_name
        client_aliases = [
          {
            dns_name = var.services["authentication"].ecs_service_connect_dns_name
            port     = var.services["authentication"].ecs_container_port_mappings[0].container_port
          }
        ]
      },
      {
        port_name      = var.services["coach"].ecs_service_connect_port_name
        discovery_name = var.services["coach"].ecs_service_connect_discovery_name
        client_aliases = [
          {
            dns_name = var.services["coach"].ecs_service_connect_dns_name
            port     = var.services["coach"].ecs_container_port_mappings[0].container_port
          }
        ]
      },
      {
        port_name      = var.services["rabbitmq"].ecs_service_connect_port_name
        discovery_name = var.services["rabbitmq"].ecs_service_connect_discovery_name
        client_aliases = [
          {
            dns_name = var.services["rabbitmq"].ecs_service_connect_dns_name
            port     = var.services["rabbitmq"].ecs_container_port_mappings[0].container_port
          }
        ]
      },
      {
        port_name      = var.services["redis"].ecs_service_connect_port_name
        discovery_name = var.services["redis"].ecs_service_connect_discovery_name
        client_aliases = [
          {
            dns_name = var.services["redis"].ecs_service_connect_dns_name
            port     = var.services["redis"].ecs_container_port_mappings[0].container_port
          }
        ]
      }
      # Note: API Gateway, Payment, and Booking services auto-discovered via Service Connect namespace
    ]
  }

  service_definitions = {
    server-1 = {
      task_cpu         = var.services["authentication"].ecs_container_cpu + var.services["coach"].ecs_container_cpu + var.services["rabbitmq"].ecs_container_cpu + var.services["redis"].ecs_container_cpu
      task_memory      = var.services["authentication"].ecs_container_memory + var.services["coach"].ecs_container_memory + var.services["rabbitmq"].ecs_container_memory + var.services["redis"].ecs_container_memory
      desired_count    = 1
      assign_public_ip = false
      placement_constraints = [
        {
          type       = "memberOf"
          expression = "attribute:service_group == server-1"
        }
      ]

      volumes = [
        {
          name      = "rabbitmq-data"
          host_path = "/var/lib/${var.project_name}/rabbitmq"
        },
        {
          name      = "redis-data"
          host_path = "/var/lib/${var.project_name}/redis"
        }
      ]

      containers = [
        {
          # RabbitMQ - deployed first as dependency
          name                  = "rabbitmq"
          image_repository_url  = var.services["rabbitmq"].ecs_container_image_repository_url
          image_tag             = var.services["rabbitmq"].ecs_container_image_tag
          cpu                   = var.services["rabbitmq"].ecs_container_cpu
          memory                = var.services["rabbitmq"].ecs_container_memory
          essential             = var.services["rabbitmq"].ecs_container_essential
          port_mappings         = var.services["rabbitmq"].ecs_container_port_mappings
          environment_variables = lookup(local.merged_service_env_vars, "rabbitmq", var.services["rabbitmq"].ecs_environment_variables)
          health_check = {
            command     = var.services["rabbitmq"].ecs_container_health_check.command
            interval    = var.services["rabbitmq"].ecs_container_health_check.interval
            timeout     = var.services["rabbitmq"].ecs_container_health_check.timeout
            retries     = var.services["rabbitmq"].ecs_container_health_check.retries
            startPeriod = var.services["rabbitmq"].ecs_container_health_check.startPeriod
          }
          mount_points = [
            {
              source_volume  = "rabbitmq-data"
              container_path = "/var/lib/rabbitmq"
            }
          ]
          depends_on = []
        },
        {
          # Redis - deployed first as dependency
          name                  = "redis"
          image_repository_url  = var.services["redis"].ecs_container_image_repository_url
          image_tag             = var.services["redis"].ecs_container_image_tag
          cpu                   = var.services["redis"].ecs_container_cpu
          memory                = var.services["redis"].ecs_container_memory
          essential             = var.services["redis"].ecs_container_essential
          port_mappings         = var.services["redis"].ecs_container_port_mappings
          environment_variables = lookup(local.merged_service_env_vars, "redis", var.services["redis"].ecs_environment_variables)
          command               = lookup(var.services["redis"], "command", null)
          health_check = {
            command     = var.services["redis"].ecs_container_health_check.command
            interval    = var.services["redis"].ecs_container_health_check.interval
            timeout     = var.services["redis"].ecs_container_health_check.timeout
            retries     = var.services["redis"].ecs_container_health_check.retries
            startPeriod = var.services["redis"].ecs_container_health_check.startPeriod
          }
          mount_points = [
            {
              source_volume  = "redis-data"
              container_path = "/data"
            }
          ]
          depends_on = []
        },
        {
          # Authentication microservice - depends on RabbitMQ and Redis
          name                  = "authentication-microservice"
          image_repository_url  = var.services["authentication"].ecs_container_image_repository_url
          image_tag             = var.services["authentication"].ecs_container_image_tag
          cpu                   = var.services["authentication"].ecs_container_cpu
          memory                = var.services["authentication"].ecs_container_memory
          essential             = var.services["authentication"].ecs_container_essential
          port_mappings         = var.services["authentication"].ecs_container_port_mappings
          environment_variables = lookup(local.merged_service_env_vars, "authentication", var.services["authentication"].ecs_environment_variables)
          health_check = {
            command     = var.services["authentication"].ecs_container_health_check.command
            interval    = var.services["authentication"].ecs_container_health_check.interval
            timeout     = var.services["authentication"].ecs_container_health_check.timeout
            retries     = var.services["authentication"].ecs_container_health_check.retries
            startPeriod = var.services["authentication"].ecs_container_health_check.startPeriod
          }
          depends_on = ["rabbitmq", "redis"]
        },
        {
          # Coach microservice - depends on RabbitMQ and Redis
          name                  = "coach-microservice"
          image_repository_url  = var.services["coach"].ecs_container_image_repository_url
          image_tag             = var.services["coach"].ecs_container_image_tag
          cpu                   = var.services["coach"].ecs_container_cpu
          memory                = var.services["coach"].ecs_container_memory
          essential             = var.services["coach"].ecs_container_essential
          port_mappings         = var.services["coach"].ecs_container_port_mappings
          environment_variables = lookup(local.merged_service_env_vars, "coach", var.services["coach"].ecs_environment_variables)
          health_check = {
            command     = var.services["coach"].ecs_container_health_check.command
            interval    = var.services["coach"].ecs_container_health_check.interval
            timeout     = var.services["coach"].ecs_container_health_check.timeout
            retries     = var.services["coach"].ecs_container_health_check.retries
            startPeriod = var.services["coach"].ecs_container_health_check.startPeriod
          }
          depends_on = ["rabbitmq", "redis"]
        }
      ]

      target_groups = []
    }
  }

  depends_on = [module.ec2]
}

# ECS Module - Server-2 (API Gateway + Payment + Booking)
# Deploys after server-1 to ensure service discovery endpoints are available
module "ecs_server2" {
  source = "./modules/ecs"

  project_name             = var.project_name
  aws_region               = var.aws_region
  vpc_id                   = module.vpc.vpc_id
  vpc_cidr                 = var.vpc_cidr
  task_subnet_ids          = [module.vpc.private_subnet_id]
  ecs_cluster_id           = module.ec2.ecs_cluster_arn
  ecs_cluster_name         = module.ec2.ecs_cluster_name
  alb_security_group_id    = module.alb.alb_sg_id
  assign_public_ip         = false
  desired_count            = 1
  service_names            = ["server-2"]
  service_discovery_domain = "${var.project_name}.${var.service_discovery_domain_suffix}"
  service_dependencies     = {}
  enable_auto_scaling      = var.enable_auto_scaling
  enable_service_connect   = var.enable_service_connect

  # Pass shared resources (same as server-1)
  shared_log_group_name     = aws_cloudwatch_log_group.ecs_logs.name
  shared_task_role_arn      = aws_iam_role.ecs_task_role.arn
  shared_execution_role_arn = aws_iam_role.ecs_execution_role.arn
  shared_task_sg_id         = aws_security_group.ecs_task_sg.id
  # Use existing namespace created above
  service_connect_namespace = var.enable_service_connect ? aws_service_discovery_private_dns_namespace.ecs_namespace[0].arn : null

  service_connect_services = {
    server-2 = [
      {
        # Publish payment-service to namespace
        port_name      = var.services["payment"].ecs_service_connect_port_name
        discovery_name = var.services["payment"].ecs_service_connect_discovery_name
        client_aliases = [
          {
            dns_name = var.services["payment"].ecs_service_connect_dns_name
            port     = var.services["payment"].ecs_container_port_mappings[0].container_port
          }
        ]
      },
      {
        # Publish booking-service to namespace
        port_name      = var.services["booking"].ecs_service_connect_port_name
        discovery_name = var.services["booking"].ecs_service_connect_discovery_name
        client_aliases = [
          {
            dns_name = var.services["booking"].ecs_service_connect_dns_name
            port     = var.services["booking"].ecs_container_port_mappings[0].container_port
          }
        ]
      },
      {
        # Publish API Gateway to namespace
        port_name      = var.services["apigateway"].ecs_service_connect_port_name
        discovery_name = var.services["apigateway"].ecs_service_connect_discovery_name
        client_aliases = [
          {
            dns_name = var.services["apigateway"].ecs_service_connect_dns_name
            port     = var.services["apigateway"].ecs_container_port_mappings[0].container_port
          }
        ]
      }
      # Note: Authentication, Coach, RabbitMQ, and Redis auto-discovered via Service Connect namespace
    ]
  }

  service_definitions = {
    server-2 = {
      task_cpu            = var.services["payment"].ecs_container_cpu + var.services["booking"].ecs_container_cpu + var.services["apigateway"].ecs_container_cpu + 64
      task_memory         = var.services["payment"].ecs_container_memory + var.services["booking"].ecs_container_memory + var.services["apigateway"].ecs_container_memory + 128
      desired_count       = 1
      assign_public_ip    = false
      enable_auto_scaling = false
      placement_constraints = [
        {
          type       = "memberOf"
          expression = "attribute:service_group == server-2"
        }
      ]

      containers = [
        {
          # Payment microservice - ensures transaction flow prior to dependent services
          name                  = "payment-microservice"
          image_repository_url  = var.services["payment"].ecs_container_image_repository_url
          image_tag             = var.services["payment"].ecs_container_image_tag
          cpu                   = var.services["payment"].ecs_container_cpu
          memory                = var.services["payment"].ecs_container_memory
          essential             = var.services["payment"].ecs_container_essential
          port_mappings         = var.services["payment"].ecs_container_port_mappings
          environment_variables = lookup(local.merged_service_env_vars, "payment", var.services["payment"].ecs_environment_variables)
          health_check = {
            command     = var.services["payment"].ecs_container_health_check.command
            interval    = var.services["payment"].ecs_container_health_check.interval
            timeout     = var.services["payment"].ecs_container_health_check.timeout
            retries     = var.services["payment"].ecs_container_health_check.retries
            startPeriod = var.services["payment"].ecs_container_health_check.startPeriod
          }
          depends_on = []
        },
        {
          # Booking microservice - handles booking workflows alongside Payment
          name                  = "booking-microservice"
          image_repository_url  = var.services["booking"].ecs_container_image_repository_url
          image_tag             = var.services["booking"].ecs_container_image_tag
          cpu                   = var.services["booking"].ecs_container_cpu
          memory                = var.services["booking"].ecs_container_memory
          essential             = var.services["booking"].ecs_container_essential
          port_mappings         = var.services["booking"].ecs_container_port_mappings
          environment_variables = lookup(local.merged_service_env_vars, "booking", var.services["booking"].ecs_environment_variables)
          health_check = {
            command     = var.services["booking"].ecs_container_health_check.command
            interval    = var.services["booking"].ecs_container_health_check.interval
            timeout     = var.services["booking"].ecs_container_health_check.timeout
            retries     = var.services["booking"].ecs_container_health_check.retries
            startPeriod = var.services["booking"].ecs_container_health_check.startPeriod
          }
          depends_on = []
        },
        {
          # API Gateway - depends on Payment and Booking services
          name                  = "api-gateway"
          image_repository_url  = var.services["apigateway"].ecs_container_image_repository_url
          image_tag             = var.services["apigateway"].ecs_container_image_tag
          cpu                   = var.services["apigateway"].ecs_container_cpu
          memory                = var.services["apigateway"].ecs_container_memory
          essential             = var.services["apigateway"].ecs_container_essential
          port_mappings         = var.services["apigateway"].ecs_container_port_mappings
          environment_variables = lookup(local.merged_service_env_vars, "apigateway", var.services["apigateway"].ecs_environment_variables)
          health_check = {
            command     = var.services["apigateway"].ecs_container_health_check.command
            interval    = var.services["apigateway"].ecs_container_health_check.interval
            timeout     = var.services["apigateway"].ecs_container_health_check.timeout
            retries     = var.services["apigateway"].ecs_container_health_check.retries
            startPeriod = var.services["apigateway"].ecs_container_health_check.startPeriod
          }
          depends_on = ["payment-microservice", "booking-microservice"]
        }
      ]

      target_groups = [
        {
          # API Gateway to ALB Target Group
          target_group_arn = module.alb.target_group_arns_map["apigateway"]
          container_name   = "api-gateway"
          container_port   = var.services["apigateway"].ecs_container_port_mappings[0].container_port
        }
      ]
    }
  }

  depends_on = [module.ecs_server1]
}

## CloudFront and Lambda@Edge modules removed
