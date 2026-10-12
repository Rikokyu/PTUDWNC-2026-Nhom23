using System.Text.Encodings.Web;

namespace CulinaryBlog.Infrastructure.Email;

public static class WelcomeEmailTemplate
{
    public static WelcomeEmailContent Create(
        string fullName,
        string applicationUrl,
        string? activationUrl = null)
    {
        var encodedName = HtmlEncoder.Default.Encode(fullName);
        var encodedApplicationUrl = HtmlEncoder.Default.Encode(applicationUrl);
        var activationSection = string.IsNullOrWhiteSpace(activationUrl)
            ? string.Empty
            : $"""
              <p style="margin:24px 0">
                <a href="{HtmlEncoder.Default.Encode(activationUrl)}" style="background:#ea580c;color:#fff;padding:12px 20px;text-decoration:none;border-radius:8px;display:inline-block">
                  Kích hoạt email
                </a>
              </p>
              """;

        var htmlBody = $"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#fff7ed;font-family:Arial,sans-serif;color:#292524">
              <div style="max-width:600px;margin:32px auto;background:#fff;padding:32px;border-radius:12px">
                <h1 style="color:#c2410c">Chào mừng {encodedName}!</h1>
                <p>Tài khoản Culinary Blog của bạn đã được tạo thành công.</p>
                <p>Bạn có thể bắt đầu khám phá và chia sẻ những công thức yêu thích ngay bây giờ.</p>
                {activationSection}
                <p style="margin:24px 0">
                  <a href="{encodedApplicationUrl}" style="background:#0f766e;color:#fff;padding:12px 20px;text-decoration:none;border-radius:8px;display:inline-block">
                    Đi đến Culinary Blog
                  </a>
                </p>
                <p style="font-size:12px;color:#78716c">Nếu nút không hoạt động, hãy mở: {encodedApplicationUrl}</p>
              </div>
            </body>
            </html>
            """;

        return new WelcomeEmailContent(
            "Chào mừng bạn đến với Culinary Blog",
            htmlBody);
    }
}

public sealed record WelcomeEmailContent(string Subject, string HtmlBody);
