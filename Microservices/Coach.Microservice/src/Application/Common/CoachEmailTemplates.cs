using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Application.Common;

public static class CoachEmailTemplates
{
    private const string CoachApprovedTemplate = """
<!DOCTYPE html>
<html lang=\"vi\">
<head>
    <meta charset=\"UTF-8\">
    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">
    <title>Tài khoản Coach đã được xác thực</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f8f9fa;
            line-height: 1.6;
            color: #333;
        }
        
        .email-container {
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(255, 140, 0, 0.1);
            overflow: hidden;
        }
        
        .header {
            background: linear-gradient(135deg, #ff8c00, #ff6600);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }
        
        .header h1 {
            font-size: 28px;
            font-weight: 700;
            margin-bottom: 5px;
            text-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .header p {
            font-size: 16px;
            opacity: 0.9;
        }
        
        .content {
            padding: 40px 30px;
        }
        
        .greeting {
            font-size: 18px;
            color: #333;
            margin-bottom: 20px;
            font-weight: 600;
        }
        
        .message {
            font-size: 16px;
            color: #555;
            margin-bottom: 20px;
            line-height: 1.7;
        }
        
        .success-badge {
            background: linear-gradient(135deg, #ff8c00, #ff6600);
            color: white;
            padding: 20px;
            border-radius: 12px;
            text-align: center;
            margin: 30px 0;
            box-shadow: 0 4px 15px rgba(255, 140, 0, 0.3);
        }
        
        .success-badge .icon {
            font-size: 48px;
            margin-bottom: 10px;
        }
        
        .success-badge h2 {
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 8px;
        }
        
        .success-badge p {
            font-size: 16px;
            opacity: 0.95;
        }
        
        .info-box {
            background-color: #ecfdf5;
            border: 2px solid #a7f3d0;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
        }
        
        .info-box h3 {
            color: #065f46;
            font-size: 16px;
            margin-bottom: 12px;
            font-weight: 600;
        }
        
        .info-box ul {
            list-style: none;
            padding-left: 0;
        }
        
        .info-box li {
            color: #047857;
            font-size: 14px;
            margin: 8px 0;
            padding-left: 24px;
            position: relative;
        }
        
        .info-box li:before {
            content: \"✓\";
            position: absolute;
            left: 0;
            color: #10b981;
            font-weight: bold;
            font-size: 16px;
        }
        
        .cta-container {
            text-align: center;
            margin: 35px 0;
        }
        
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #ff8c00, #ff6600);
            color: white;
            text-decoration: none;
            padding: 16px 40px;
            font-size: 18px;
            font-weight: 600;
            border-radius: 50px;
            box-shadow: 0 4px 15px rgba(255, 140, 0, 0.3);
            transition: all 0.3s ease;
            text-transform: uppercase;
            letter-spacing: 1px;
        }
        
        .cta-button:hover {
            background: linear-gradient(135deg, #ff6600, #ff4500);
            box-shadow: 0 6px 20px rgba(255, 140, 0, 0.4);
            transform: translateY(-2px);
        }
        
        .tips-section {
            background-color: #fff7ed;
            border-radius: 8px;
            padding: 25px;
            margin: 25px 0;
        }
        
        .tips-section h3 {
            color: #ff6600;
            font-size: 18px;
            margin-bottom: 15px;
            font-weight: 600;
        }
        
        .tips-section ul {
            padding-left: 20px;
        }
        
        .tips-section li {
            color: #666;
            font-size: 14px;
            margin: 10px 0;
            line-height: 1.6;
        }
        
        .support-section {
            background-color: #f8f9fa;
            border-radius: 8px;
            padding: 25px;
            margin: 25px 0;
            text-align: center;
        }
        
        .support-section h3 {
            color: #ff6600;
            font-size: 18px;
            margin-bottom: 15px;
        }
        
        .contact-info {
            font-size: 14px;
            color: #666;
            margin: 5px 0;
        }
        
        .contact-info a {
            color: #ff6600;
            text-decoration: none;
            font-weight: 500;
        }
        
        .contact-info a:hover {
            color: #ff4500;
            text-decoration: underline;
        }
        
        .footer {
            background-color: #2d3748;
            color: #a0aec0;
            padding: 25px 30px;
            text-align: center;
        }
        
        .footer h4 {
            color: #ff8c00;
            font-size: 20px;
            margin-bottom: 10px;
        }
        
        .footer p {
            font-size: 14px;
            margin: 5px 0;
        }
        
        .divider {
            height: 3px;
            background: linear-gradient(90deg, #ff8c00, #ff6600, #ff8c00);
            margin: 20px 0;
            border-radius: 2px;
        }
        
        @media (max-width: 600px) {
            .email-container {
                margin: 10px;
                border-radius: 8px;
            }
            
            .content {
                padding: 25px 20px;
            }
            
            .header {
                padding: 25px 20px;
            }
            
            .header h1 {
                font-size: 24px;
            }
            
            .cta-button {
                padding: 14px 30px;
                font-size: 16px;
            }
            
            .success-badge h2 {
                font-size: 20px;
            }
        }
    </style>
</head>
<body>
    <div class=\"email-container\">
        <div class=\"header\">
            <h1>Fitverse</h1>
            <p>Chúc mừng! Bạn đã trở thành Coach chính thức</p>
        </div>
        
        <div class=\"content\">
            <div class=\"greeting\">
                Kính chào Coach {{CoachName}},
            </div>
            
            <div class=\"success-badge\">
                <div class=\"icon\">🎉</div>
                <h2>Tài khoản đã được xác thực!</h2>
                <p>Hồ sơ của bạn đã sẵn sàng hiển thị công khai</p>
            </div>
            
            <div class=\"message\">
                Chúng tôi vui mừng thông báo rằng tài khoản Coach của bạn đã được xác thực thành công! Từ giờ, hồ sơ của bạn sẽ được <strong>hiển thị công khai</strong> trên nền tảng Fitverse và khách hàng có thể tìm thấy và đặt lịch với bạn.
            </div>
            
            <div class=\"info-box\">
                <h3>Những gì bạn có thể làm ngay bây giờ:</h3>
                <ul>
                    <li>Hồ sơ của bạn đang hiển thị công khai với khách hàng</li>
                    <li>Khách hàng có thể xem thông tin và đặt lịch tập với bạn</li>
                    <li>Bạn sẽ nhận được thông báo khi có lịch hẹn mới</li>
                    <li>Có thể quản lý lịch trình và khách hàng của mình</li>
                </ul>
            </div>
            
            <div class=\"cta-container\">
                <a href=\"{{DashboardLink}}\" class=\"cta-button\">Truy cập Dashboard</a>
            </div>
            
            <div class=\"divider\"></div>
            
            <div class=\"tips-section\">
                <h3>💡 Mẹo để thu hút khách hàng:</h3>
                <ul>
                    <li>Cập nhật ảnh đại diện chuyên nghiệp và hình ảnh minh họa</li>
                    <li>Hoàn thiện mô tả về kinh nghiệm và chuyên môn của bạn</li>
                    <li>Thêm các chứng chỉ và thành tích đạt được</li>
                    <li>Đặt giá dịch vụ hợp lý và rõ ràng</li>
                    <li>Phản hồi nhanh chóng các yêu cầu từ khách hàng</li>
                </ul>
            </div>
            
            <div class=\"support-section\">
                <h3>Cần hỗ trợ?</h3>
                <div class=\"contact-info\">
                    Email: <a href=\"mailto:contact.nextgenzcompany@gmail.com\">contact.nextgenzcompany@gmail.com</a>
                </div>
                <div class=\"contact-info\">
                    Điện thoại: <a href=\"tel:0869246429\">0869246429</a>
                </div>
            </div>
        </div>
        
        <div class=\"footer\">
            <h4>Fitverse</h4>
            <p>Chúc bạn thành công trên hành trình cùng Fitverse</p>
            <p style=\"margin-top: 15px; font-size: 12px; opacity: 0.7;\">
                © 2025 Fitverse. Tất cả các quyền được bảo lưu.
            </p>
        </div>
    </div>
</body>
</html>
""";

    private const string CoachRejectedTemplate = """
<!DOCTYPE html>
<html lang=\"vi\">
<head>
    <meta charset=\"UTF-8\">
    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">
    <title>Tài khoản Coach - Cần bổ sung thông tin</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f8f9fa;
            line-height: 1.6;
            color: #333;
        }
        
        .email-container {
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(239, 68, 68, 0.1);
            overflow: hidden;
        }
        
        .header {
            background: linear-gradient(135deg, #ef4444, #dc2626);
            padding: 30px 20px;
            text-align: center;
            color: white;
        }
        
        .header h1 {
            font-size: 28px;
            font-weight: 700;
            margin-bottom: 5px;
            text-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .header p {
            font-size: 16px;
            opacity: 0.9;
        }
        
        .content {
            padding: 40px 30px;
        }
        
        .greeting {
            font-size: 18px;
            color: #333;
            margin-bottom: 20px;
            font-weight: 600;
        }
        
        .message {
            font-size: 16px;
            color: #555;
            margin-bottom: 20px;
            line-height: 1.7;
        }
        
        .alert-badge {
            background: linear-gradient(135deg, #ef4444, #dc2626);
            color: white;
            padding: 20px;
            border-radius: 12px;
            text-align: center;
            margin: 30px 0;
            box-shadow: 0 4px 15px rgba(239, 68, 68, 0.3);
        }
        
        .alert-badge .icon {
            font-size: 48px;
            margin-bottom: 10px;
        }
        
        .alert-badge h2 {
            font-size: 24px;
            font-weight: 700;
            margin-bottom: 8px;
        }
        
        .alert-badge p {
            font-size: 16px;
            opacity: 0.95;
        }
        
        .warning-box {
            background-color: #fef2f2;
            border: 2px solid #fecaca;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
        }
        
        .warning-box h3 {
            color: #991b1b;
            font-size: 16px;
            margin-bottom: 12px;
            font-weight: 600;
        }
        
        .warning-box ul {
            list-style: none;
            padding-left: 0;
        }
        
        .warning-box li {
            color: #b91c1c;
            font-size: 14px;
            margin: 8px 0;
            padding-left: 24px;
            position: relative;
        }
        
        .warning-box li:before {
            content: \"✗\";
            position: absolute;
            left: 0;
            color: #ef4444;
            font-weight: bold;
            font-size: 16px;
        }
        
        .info-box {
            background-color: #eff6ff;
            border: 2px solid #bfdbfe;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
        }
        
        .info-box h3 {
            color: #1e40af;
            font-size: 16px;
            margin-bottom: 12px;
            font-weight: 600;
        }
        
        .info-box ul {
            list-style: none;
            padding-left: 0;
        }
        
        .info-box li {
            color: #1e3a8a;
            font-size: 14px;
            margin: 8px 0;
            padding-left: 24px;
            position: relative;
        }
        
        .info-box li:before {
            content: \"→\";
            position: absolute;
            left: 0;
            color: #3b82f6;
            font-weight: bold;
            font-size: 16px;
        }
        
        .tips-section {
            background-color: #fff7ed;
            border-radius: 8px;
            padding: 25px;
            margin: 25px 0;
        }
        
        .tips-section h3 {
            color: #ff6600;
            font-size: 18px;
            margin-bottom: 15px;
            font-weight: 600;
        }
        
        .tips-section ul {
            padding-left: 20px;
        }
        
        .tips-section li {
            color: #666;
            font-size: 14px;
            margin: 10px 0;
            line-height: 1.6;
        }
        
        .support-section {
            background-color: #f8f9fa;
            border-radius: 8px;
            padding: 25px;
            margin: 25px 0;
            text-align: center;
        }
        
        .support-section h3 {
            color: #ff6600;
            font-size: 18px;
            margin-bottom: 15px;
        }
        
        .contact-info {
            font-size: 14px;
            color: #666;
            margin: 5px 0;
        }
        
        .contact-info a {
            color: #ff6600;
            text-decoration: none;
            font-weight: 500;
        }
        
        .contact-info a:hover {
            color: #ff4500;
            text-decoration: underline;
        }
        
        .footer {
            background-color: #2d3748;
            color: #a0aec0;
            padding: 25px 30px;
            text-align: center;
        }
        
        .footer h4 {
            color: #ff8c00;
            font-size: 20px;
            margin-bottom: 10px;
        }
        
        .footer p {
            font-size: 14px;
            margin: 5px 0;
        }
        
        .divider {
            height: 3px;
            background: linear-gradient(90deg, #ef4444, #dc2626, #ef4444);
            margin: 20px 0;
            border-radius: 2px;
        }
        
        @media (max-width: 600px) {
            .email-container {
                margin: 10px;
                border-radius: 8px;
            }
            
            .content {
                padding: 25px 20px;
            }
            
            .header {
                padding: 25px 20px;
            }
            
            .header h1 {
                font-size: 24px;
            }
            
            .alert-badge h2 {
                font-size: 20px;
            }
        }
    </style>
</head>
<body>
    <div class=\"email-container\">
        <div class=\"header\">
            <h1>Fitverse</h1>
            <p>Yêu cầu xác thực cần được bổ sung thông tin</p>
        </div>
        
        <div class=\"content\">
            <div class=\"greeting\">
                Kính chào {{CoachName}},
            </div>
            
            <div class=\"alert-badge\">
                <div class=\"icon\">⚠️</div>
                <h2>Xác thực chưa thành công</h2>
                <p>Hồ sơ của bạn cần được bổ sung thêm thông tin</p>
            </div>
            
            <div class=\"message\">
                Cảm ơn bạn đã đăng ký làm Coach trên nền tảng Fitverse. Tuy nhiên, sau khi xem xét hồ sơ của bạn, chúng tôi nhận thấy một số thông tin <strong>chưa đầy đủ hoặc chưa đáp ứng yêu cầu</strong> để có thể xác thực tài khoản.
            </div>
            
            <div class=\"warning-box\">
                <h3>Các vấn đề cần khắc phục:</h3>
                <ul>
                    {{ReasonsList}}
                </ul>
            </div>
            
            <div class=\"info-box\">
                <h3>Các bước tiếp theo:</h3>
                <ul>
                    <li>Đăng nhập vào tài khoản của bạn</li>
                    <li>Cập nhật đầy đủ các thông tin còn thiếu</li>
                    <li>Bổ sung các chứng chỉ/tài liệu cần thiết</li>
                    <li>Gửi lại yêu cầu xác thực</li>
                </ul>
            </div>
            
            <div class=\"divider\"></div>
            
            <div class=\"tips-section\">
                <h3>💡 Hướng dẫn hoàn thiện hồ sơ:</h3>
                <ul>
                    <li><strong>Chứng chỉ:</strong> Tải lên bản scan rõ nét các chứng chỉ đào tạo PT/Fitness còn hiệu lực</li>
                    <li><strong>Kinh nghiệm:</strong> Mô tả chi tiết về số năm kinh nghiệm và nơi làm việc</li>
                    <li><strong>Ảnh đại diện:</strong> Sử dụng ảnh chân dung chuyên nghiệp, rõ mặt</li>
                    <li><strong>Mô tả dịch vụ:</strong> Viết rõ các dịch vụ bạn cung cấp và chuyên môn</li>
                    <li><strong>Thông tin liên hệ:</strong> Cung cấp số điện thoại và email chính xác</li>
                </ul>
            </div>
            
            <div class=\"message\">
                Chúng tôi mong muốn hợp tác cùng bạn và tin rằng sau khi hoàn thiện hồ sơ, bạn sẽ trở thành một Coach xuất sắc trên nền tảng Fitverse. Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.
            </div>
            
            <div class=\"support-section\">
                <h3>Cần hỗ trợ?</h3>
                <div class=\"contact-info\">
                    Email: <a href=\"mailto:contact.nextgenzcompany@gmail.com\">contact.nextgenzcompany@gmail.com</a>
                </div>
                <div class=\"contact-info\">
                    Điện thoại: <a href=\"tel:0869246429\">0869246429</a>
                </div>
            </div>
        </div>
        
        <div class=\"footer\">
            <h4>Fitverse</h4>
            <p>Chúng tôi sẵn sàng hỗ trợ bạn hoàn thiện hồ sơ</p>
            <p style=\"margin-top: 15px; font-size: 12px; opacity: 0.7;\">
                © 2025 Fitverse. Tất cả các quyền được bảo lưu.
            </p>
        </div>
    </div>
</body>
</html>
""";

    public static string BuildCoachApprovedEmail(string coachName, string dashboardUrl)
    {
        var safeName = string.IsNullOrWhiteSpace(coachName) ? "Coach" : coachName.Trim();
        var safeDashboardUrl = string.IsNullOrWhiteSpace(dashboardUrl) ? "#" : dashboardUrl.Trim();

        return CoachApprovedTemplate
            .Replace("{{CoachName}}", safeName, StringComparison.Ordinal)
            .Replace("{{DashboardLink}}", safeDashboardUrl, StringComparison.Ordinal);
    }

    public static string BuildCoachRejectedEmail(string coachName, IEnumerable<string>? reasons)
    {
        var safeName = string.IsNullOrWhiteSpace(coachName) ? "Coach" : coachName.Trim();
        var reasonItems = (reasons ?? Array.Empty<string>())
            .Where(reason => !string.IsNullOrWhiteSpace(reason))
            .Select(reason => reason.Trim())
            .ToArray();

        if (reasonItems.Length == 0)
        {
            reasonItems = new[] { "Hồ sơ của bạn cần bổ sung thêm thông tin để đáp ứng yêu cầu xác thực." };
        }

        var reasonsHtml = string.Join(Environment.NewLine, reasonItems.Select(r => $"<li>{WebUtility.HtmlEncode(r)}</li>"));

        return CoachRejectedTemplate
            .Replace("{{CoachName}}", safeName, StringComparison.Ordinal)
            .Replace("{{ReasonsList}}", reasonsHtml, StringComparison.Ordinal);
    }
}
