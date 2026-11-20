using System.Text.Encodings.Web;

namespace CulinaryBlog.Infrastructure.Email;

public static class WelcomeEmailTemplate
{
    public const string LogoContentId = "culinary-blog-logo";

    public static WelcomeEmailContent Create(
        string displayName,
        string applicationUrl,
        string? activationUrl = null)
    {
        var encodedName = HtmlEncoder.Default.Encode(displayName);
        var encodedApplicationUrl = HtmlEncoder.Default.Encode(applicationUrl);
        var hasActivation = !string.IsNullOrWhiteSpace(activationUrl);
        var actionUrl = hasActivation
            ? HtmlEncoder.Default.Encode(activationUrl!)
            : encodedApplicationUrl;
        var actionLabel = hasActivation
            ? "Kích hoạt email"
            : "Đi đến Culinary Blog";
        var activationSection = hasActivation
            ? $"""
              <p style="margin:0 0 18px;color:#68665f;font-size:14px;line-height:24px">
                Sau khi kích hoạt email, bạn có thể khám phá và chia sẻ công thức tại
                <a href="{encodedApplicationUrl}" style="color:#a94f2d;text-decoration:underline">Culinary Blog</a>.
              </p>
              """
            : string.Empty;

        var htmlBody = $"""
            <!doctype html>
            <html lang="vi">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Chào mừng đến với Culinary Blog</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f5f2eb;color:#29271f;font-family:Arial,Helvetica,sans-serif">
              <span style="display:none!important;visibility:hidden;opacity:0;height:0;width:0;overflow:hidden;color:transparent">
                Tài khoản của bạn đã sẵn sàng. Cùng bắt đầu hành trình nấu ăn tại Culinary Blog.
              </span>
              <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="background-color:#f5f2eb">
                <tr><td align="center" style="padding:36px 16px 48px">
                  <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="600" style="width:100%;max-width:600px;background-color:#fffdf9;border:1px solid #e8e2d7;border-radius:18px;overflow:hidden">
                    <tr><td style="height:5px;background-color:#53633d;font-size:0;line-height:0">&nbsp;</td></tr>
                    <tr><td align="center" style="padding:32px 24px 28px;background-color:#fffdf9">
                      <img src="cid:{LogoContentId}" alt="Culinary Blog" width="88" height="88" style="display:block;width:88px;height:88px;border:0;outline:none;text-decoration:none">
                      <p style="margin:12px 0 0;color:#29271f;font-family:Georgia,serif;font-size:24px;font-weight:bold;letter-spacing:-0.5px">Culinary <span style="color:#a94f2d">Blog</span></p>
                      <p style="margin:6px 0 0;color:#747c64;font-size:11px;font-weight:bold;letter-spacing:2px">NẤU ĂN • CHIA SẺ • KẾT NỐI</p>
                    </td></tr>
                    <tr><td align="center" style="padding:34px 32px 32px;background-color:#f1f2e9;border-top:1px solid #e6e8dc;border-bottom:1px solid #e6e8dc">
                      <p style="margin:0 0 10px;color:#64724c;font-size:11px;font-weight:bold;letter-spacing:2px">CHÀO MỪNG THÀNH VIÊN MỚI</p>
                      <h1 style="margin:0;color:#29271f;font-family:Arial,Helvetica,sans-serif;font-size:32px;line-height:42px;font-weight:bold">Chào mừng {encodedName}!</h1>
                      <p style="margin:14px 0 0;color:#5f6754;font-size:16px;line-height:26px">Một căn bếp mới đang chờ bạn khám phá.</p>
                    </td></tr>
                    <tr><td style="padding:34px 36px 36px">
                      <p style="margin:0 0 14px;color:#4d4b44;font-size:16px;line-height:27px">Tài khoản Culinary Blog của bạn đã được tạo thành công.</p>
                      <p style="margin:0 0 28px;color:#4d4b44;font-size:16px;line-height:27px">Hãy tìm món ngon cho hôm nay, lưu lại công thức bạn yêu thích và chia sẻ hương vị riêng với mọi người.</p>
                      <table role="presentation" cellpadding="0" cellspacing="0" border="0"><tr>
                        <td align="center" style="border-radius:10px;background-color:#a94f2d">
                          <a href="{actionUrl}" style="display:inline-block;padding:15px 26px;color:#ffffff;font-size:15px;font-weight:bold;line-height:20px;text-decoration:none">{actionLabel} &rarr;</a>
                        </td>
                      </tr></table>
                      <p style="margin:24px 0 6px;color:#817d73;font-size:12px;line-height:20px">Nếu nút không hoạt động, hãy mở đường dẫn:</p>
                      <p style="margin:0 0 20px;word-break:break-all;font-size:12px;line-height:20px"><a href="{actionUrl}" style="color:#a94f2d;text-decoration:underline">{actionUrl}</a></p>
                      {activationSection}
                      <p style="margin:0;padding-top:20px;border-top:1px solid #eee9df;color:#53633d;font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:600;line-height:25px">Món ngon bắt đầu từ những điều giản dị. Hẹn gặp bạn trong căn bếp Culinary Blog!</p>
                    </td></tr>
                    <tr><td align="center" style="padding:20px 30px;background-color:#f8f5ee;border-top:1px solid #eee9df;color:#817d73;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:20px">
                      Email này được gửi vì bạn vừa đăng ký Culinary Blog.<br>Nếu không phải bạn, hãy bỏ qua email này.
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        return new WelcomeEmailContent(
            "Chào mừng bạn đến với Culinary Blog",
            htmlBody);
    }
}

public sealed record WelcomeEmailContent(string Subject, string HtmlBody);
