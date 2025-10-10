using System;
using System.Net;
using System.Linq;
using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Validator;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Common;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Unit>
{
    private readonly IAuthenticationRepository _authen;
    private readonly IEmailSender _email;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IAuthenticationRepository authen,
        IEmailSender email,
        IJwtTokenGenerator jwt,
        IUnitOfWork unitOfWork)
    {
        _authen = authen;
        _email = email;
        _jwt = jwt;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _authen.ExistsByEmailAsync(request.Email.Trim().ToLower(), ct))
        {
            return Result.Failure<Unit>(new Error("500", "Email existed"));
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLower(),
            FullName = request.FullName,
            IsActive = true,
            EmailConfirmed = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var roleRepository = _unitOfWork.Repository<Role>();
        var customerRoleName = RoleType.Customer.GetDisplayName();
        var normalizedRoleName = customerRoleName.ToLowerInvariant();
        var customerRole = (await roleRepository.FindAsync(
            r => r.DisplayName != null && r.DisplayName.ToLower() == normalizedRoleName,
            ct)).FirstOrDefault();

        if (customerRole is null)
        {
            customerRole = new Role
            {
                Id = Guid.NewGuid(),
                DisplayName = customerRoleName
            };
            await roleRepository.AddAsync(customerRole, ct);
        }

        user.Roles.Add(customerRole);
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        await _authen.AddAsync(user, ct);

        var emailToken = _jwt.CreatePurposeToken(
            user.Id,
            "email_confirm",
            TimeSpan.FromMinutes(30));

        var link = $"{request.ConfirmBaseUrl}?token={WebUtility.UrlEncode(emailToken)}";

        var subject = "Xác nhận đăng ký tài khoản";
        var displayName = string.IsNullOrWhiteSpace(user.FullName)
            ? user.Email!
            : user.FullName!;

        var body = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Xác nhận đăng ký tài khoản</title>
  <style>
    *{{margin:0;padding:0;box-sizing:border-box}}
    body{{font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;background:#f8f9fa;line-height:1.6;color:#333}}
    .email-container{{max-width:600px;margin:20px auto;background:#fff;border-radius:12px;box-shadow:0 4px 20px rgba(255,140,0,.1);overflow:hidden}}
    .header{{background:linear-gradient(135deg,#ff8c00,#ff6600);padding:30px 20px;text-align:center;color:#fff}}
    .header h1{{font-size:28px;font-weight:700;margin-bottom:5px;text-shadow:0 2px 4px rgba(0,0,0,.1)}}
    .header p{{font-size:16px;opacity:.9}}
    .content{{padding:40px 30px}}
    .greeting{{font-size:18px;color:#333;margin-bottom:20px;font-weight:600}}
    .message{{font-size:16px;color:#555;margin-bottom:30px;line-height:1.7}}
    .cta-container{{text-align:center;margin:35px 0}}
    .cta-button{{display:inline-block;background:linear-gradient(135deg,#ff8c00,#ff6600);color:#fff;text-decoration:none;padding:16px 40px;font-size:18px;font-weight:600;border-radius:50px;box-shadow:0 4px 15px rgba(255,140,0,.3);transition:.3s;text-transform:uppercase;letter-spacing:1px}}
    .cta-button:hover{{background:linear-gradient(135deg,#ff6600,#ff4500);box-shadow:0 6px 20px rgba(255,140,0,.4);transform:translateY(-2px)}}
    .info-box{{background:#fff7ed;border:2px solid #fed7aa;border-radius:8px;padding:20px;margin:25px 0}}
    .info-box p{{color:#c2410c;font-size:14px;margin:0;font-weight:500}}
    .support-section{{background:#f8f9fa;border-radius:8px;padding:25px;margin:25px 0;text-align:center}}
    .support-section h3{{color:#ff6600;font-size:18px;margin-bottom:15px}}
    .contact-info{{font-size:14px;color:#666;margin:5px 0}}
    .contact-info a{{color:#ff6600;text-decoration:none;font-weight:500}}
    .contact-info a:hover{{color:#ff4500;text-decoration:underline}}
    .footer{{background:#2d3748;color:#a0aec0;padding:25px 30px;text-align:center}}
    .footer h4{{color:#ff8c00;font-size:20px;margin-bottom:10px}}
    .footer p{{font-size:14px;margin:5px 0}}
    .divider{{height:3px;background:linear-gradient(90deg,#ff8c00,#ff6600,#ff8c00);margin:20px 0;border-radius:2px}}
    @media (max-width:600px){{
      .email-container{{margin:10px;border-radius:8px}}
      .content{{padding:25px 20px}}
      .header{{padding:25px 20px}}
      .header h1{{font-size:24px}}
      .cta-button{{padding:14px 30px;font-size:16px}}
    }}
  </style>
</head>
<body>
  <div class=""email-container"">
    <div class=""header"">
      <h1>Fitverse</h1>
      <p>Chào mừng bạn đến với cộng đồng của chúng tôi</p>
    </div>
    <div class=""content"">
      <div class=""greeting"">Kính chào {displayName},</div>
      <div class=""message"">
        Cảm ơn bạn đã đăng ký tài khoản tại <strong>Fitverse</strong>! Chúng tôi rất vui mừng chào đón bạn.
      </div>
      <div class=""message"">
        Để hoàn tất quá trình đăng ký và kích hoạt tài khoản, vui lòng nhấn vào nút bên dưới:
      </div>
      <div class=""cta-container"">
        <a href=""{link}"" class=""cta-button"">Xác nhận tài khoản</a>
      </div>
      <div class=""info-box"">
        <p><strong>Lưu ý:</strong> Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này. Link xác nhận sẽ hết hiệu lực sau 2 giờ.</p>
      </div>
      <div class=""divider""></div>
      <div class=""support-section"">
        <h3>Cần hỗ trợ?</h3>
        <div class=""contact-info"">Email: <a href=""mailto:contact.nextgenzcompany@gmail.com"">contact.nextgenzcompany@gmail.com</a></div>
        <div class=""contact-info"">Điện thoại: <a href=""tel:0869246429"">0869246429</a></div>
      </div>
    </div>
    <div class=""footer"">
      <h4>Fitverse</h4>
      <p>Trân trọng cảm ơn</p>
      <p style=""margin-top:15px;font-size:12px;opacity:.7;"">© 2025 Fitverse. Tất cả các quyền được bảo lưu.</p>
    </div>
  </div>
</body>
</html>";

        await _email.SendAsync(user.Email!, subject, body, ct);
        return Result.Success(Unit.Value);

        // await _email.SendAsync(
        //     user.Email!,
        //     "X�c th?c email",
        //     $"Nh?p �? x�c th?c: {link}",
        //     ct);
        // return Result.Success(Unit.Value);
    }
}