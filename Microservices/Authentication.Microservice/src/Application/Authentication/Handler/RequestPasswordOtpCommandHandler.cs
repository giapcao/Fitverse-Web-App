using Application.Abstractions.Interface;
using Application.Abstractions.Messaging;
using Application.Authentication.Command;
using Application.Common;
using Domain.IRepositories;
using Infrastructure.Common;
using MediatR;
using Microsoft.Extensions.Options;
using SharedLibrary.Common.ResponseModel;

namespace Application.Authentication.Handler;

public sealed class RequestPasswordOtpCommandHandler 
    : ICommandHandler<RequestPasswordOtpCommand, Unit>
{
    private readonly IAuthenticationRepository _auth;
    private readonly IOtpStore _otp;
    private readonly IEmailSender _emailSender;
    private readonly IOptions<OtpOptions> _opt;

    public RequestPasswordOtpCommandHandler(
        IAuthenticationRepository auth,
        IOtpStore otp,
        IEmailSender emailSender,
        IOptions<OtpOptions> opt)
    {
        _auth = auth; _otp = otp; _emailSender = emailSender; _opt = opt;
    }

    public async Task<Result<Unit>> Handle(RequestPasswordOtpCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        
        var user = await _auth.FindByEmailAsync(email, ct);
        if (user is null) return Unit.Value;
        
        if (!await _otp.CanIssueAsync(email, ct))
            return Unit.Value;

        var otp = await _otp.IssueAsync(email, user.Id, TimeSpan.FromMinutes(_opt.Value.TtlMinutes), ct);
        var emailSubject = "Mã OTP đặt lại mật khẩu";
        var html = $@"
        <!DOCTYPE html>
        <html lang=""vi"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Mã OTP đặt lại mật khẩu</title>
            <style>
                * {{
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }}
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background-color: #f8f9fa;
                    line-height: 1.6;
                    color: #333;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 20px auto;
                    background-color: #ffffff;
                    border-radius: 12px;
                    box-shadow: 0 4px 20px rgba(255, 140, 0, 0.1);
                    overflow: hidden;
                }}
                .header {{
                    background: linear-gradient(135deg, #ff8c00, #ff6600);
                    padding: 30px 20px;
                    text-align: center;
                    color: white;
                }}
                .header h1 {{
                    font-size: 28px;
                    font-weight: 700;
                    margin-bottom: 5px;
                    text-shadow: 0 2px 4px rgba(0,0,0,0.1);
                }}
                .header p {{
                    font-size: 16px;
                    opacity: 0.9;
                }}
                .content {{
                    padding: 40px 30px;
                }}
                .greeting {{
                    font-size: 18px;
                    color: #333;
                    margin-bottom: 20px;
                    font-weight: 600;
                }}
                .message {{
                    font-size: 16px;
                    color: #555;
                    margin-bottom: 20px;
                    line-height: 1.7;
                }}
                .otp-box {{
                    text-align: center;
                    margin: 30px 0 10px 0;
                }}
                .otp-code {{
                    display: inline-block;
                    font-size: 32px;
                    font-weight: 800;
                    letter-spacing: 6px;
                    padding: 16px 26px;
                    border-radius: 12px;
                    background: #fff7ed;
                    border: 2px dashed #fb923c;
                    color: #c2410c;
                }}
                .info-box {{
                    background-color: #fff7ed;
                    border: 2px solid #fed7aa;
                    border-radius: 8px;
                    padding: 16px;
                    margin: 20px 0;
                }}
                .info-box p {{
                    color: #c2410c;
                    font-size: 14px;
                    margin: 0;
                    font-weight: 500;
                }}
                .support-section {{
                    background-color: #f8f9fa;
                    border-radius: 8px;
                    padding: 20px;
                    margin: 25px 0;
                    text-align: center;
                }}
                .support-section h3 {{
                    color: #ff6600;
                    font-size: 18px;
                    margin-bottom: 10px;
                }}
                .contact-info {{
                    font-size: 14px;
                    color: #666;
                    margin: 5px 0;
                }}
                .contact-info a {{
                    color: #ff6600;
                    text-decoration: none;
                    font-weight: 500;
                }}
                .contact-info a:hover {{
                    color: #ff4500;
                    text-decoration: underline;
                }}
                .divider {{
                    height: 3px;
                    background: linear-gradient(90deg, #ff8c00, #ff6600, #ff8c00);
                    margin: 20px 0;
                    border-radius: 2px;
                }}
                .footer {{
                    background-color: #2d3748;
                    color: #a0aec0;
                    padding: 25px 30px;
                    text-align: center;
                }}
                .footer h4 {{
                    color: #ff8c00;
                    font-size: 20px;
                    margin-bottom: 10px;
                }}
                .footer p {{
                    font-size: 14px;
                    margin: 5px 0;
                }}
                @media (max-width: 600px) {{
                    .email-container {{
                        margin: 10px;
                        border-radius: 8px;
                    }}
                    .content {{
                        padding: 25px 20px;
                    }}
                    .header {{
                        padding: 25px 20px;
                    }}
                    .header h1 {{
                        font-size: 24px;
                    }}
                    .otp-code {{
                        font-size: 26px;
                        letter-spacing: 5px;
                    }}
                }}
            </style>
        </head>
        <body>
            <div class=""email-container"">
                <!-- Header -->
                <div class=""header"">
                    <h1>Fitverse</h1>
                    <p>Bảo mật tài khoản của bạn</p>
                </div>

                <!-- Main Content -->
                <div class=""content"">
                    <div class=""greeting"">
                        Xin chào,
                    </div>

                    <div class=""message"">
                        Chúng tôi nhận được yêu cầu <strong>đặt lại mật khẩu</strong> cho tài khoản của bạn.
                        Vui lòng sử dụng mã OTP bên dưới để hoàn tất xác thực.
                    </div>

                    <div class=""otp-box"">
                        <div class=""otp-code"">{otp}</div>
                    </div>

                    <div class=""message"">
                        Mã OTP có hiệu lực trong <strong>{_opt.Value.TtlMinutes} phút</strong> kể từ khi email này được gửi.
                        Vui lòng không chia sẻ mã với bất kỳ ai.
                    </div>

                    <div class=""info-box"">
                        <p><strong>Lưu ý:</strong> Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ hỗ trợ để được giúp đỡ.</p>
                    </div>

                    <div class=""divider""></div>

                    <div class=""support-section"">
                        <h3>Cần hỗ trợ?</h3>
                        <div class=""contact-info"">
                            Email: <a href=""mailto:contact.nextgenzcompany@gmail.com"">contact.nextgenzcompany@gmail.com</a>
                        </div>
                        <div class=""contact-info"">
                            Điện thoại: <a href=""tel:0869246429"">0869 246 429</a>
                        </div>
                    </div>
                </div>

                <!-- Footer -->
                <div class=""footer"">
                    <h4>Fitverse</h4>
                    <p>Trân trọng cảm ơn</p>
                    <p style=""margin-top: 15px; font-size: 12px; opacity: 0.7;"">© 2025 Fitverse. Tất cả các quyền được bảo lưu.</p>
                </div>
            </div>
        </body>
        </html>";
        await _emailSender.SendAsync(email, emailSubject, html, ct);


        return Unit.Value;
    }
}