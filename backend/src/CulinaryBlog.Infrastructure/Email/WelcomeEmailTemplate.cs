
using System.Text.Encodings.Web;

namespace CulinaryBlog.Infrastructure.Email;

public static class WelcomeEmailTemplate
{
    public static WelcomeEmailContent Create(
        string displayName,
        string applicationUrl,
        string? activationUrl = null,
        string? logoUrl = null)
    {
        // Mã hóa dữ liệu trước khi đưa vào HTML
        var name = HtmlEncoder.Default.Encode(displayName);
        var appUrl = HtmlEncoder.Default.Encode(applicationUrl);

// Email clients load this image from its absolute public URL.
        var logoSection = string.IsNullOrWhiteSpace(logoUrl)
            ? ""
            : $"""
              <img
                src="{HtmlEncoder.Default.Encode(logoUrl)}"
                alt="Culinary Blog Logo"
                width="90"
                style="display:block;width:90px;height:auto;
                       margin:0 auto 14px;border:0;"
              />
              """;

        // Nút kích hoạt chỉ hiển thị nếu có URL
        var activationSection = string.IsNullOrWhiteSpace(activationUrl)
            ? ""
            : $"""
              <tr>
                <td style="padding:8px 32px 20px;text-align:center;">
                  <p style="font-size:14px;color:#57534e;
                            line-height:1.7;">
                    Vui lòng xác thực địa chỉ email để
                    hoàn tất quá trình kích hoạt tài khoản.
                  </p>

                  <a href="{HtmlEncoder.Default.Encode(activationUrl)}"
                     style="display:inline-block;
                            background:#0f766e;color:#ffffff;
                            padding:14px 28px;
                            font-size:15px;font-weight:bold;
                            text-decoration:none;border-radius:8px;">
                    Xác thực email của tôi
                  </a>
                </td>
              </tr>
              """;

        var htmlBody = $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport"
                      content="width=device-width, initial-scale=1.0">
                <meta name="color-scheme" content="light">
                <title>Chào mừng đến với Culinary Blog</title>
            </head>

            <body style="margin:0;padding:0;
                         background:#f8f0e6;
                         font-family:Arial,Helvetica,sans-serif;">

                <!-- Preview text -->
                <div style="display:none;font-size:1px;
                            color:#f8f0e6;line-height:1px;
                            max-height:0;max-width:0;
                            opacity:0;overflow:hidden;">
                    Chào mừng bạn đến với Culinary Blog!
                    Hành trình khám phá ẩm thực bắt đầu từ đây.
                </div>

                <table role="presentation"
                       cellpadding="0"
                       cellspacing="0"
                       border="0"
                       width="100%"
                       style="background:#f8f0e6;">
                    <tr>
                        <td align="center"
                            style="padding:32px 12px;">

                            <!-- MAIN CONTAINER -->
                            <table role="presentation"
                                   cellpadding="0"
                                   cellspacing="0"
                                   border="0"
                                   width="100%"
                                   style="max-width:600px;
                                          background:#ffffff;
                                          border-radius:16px;
                                          overflow:hidden;">

                                <!-- HEADER -->
                                <tr>
                                    <td align="center"
                                        style="background:#8f3d25;
                                               padding:38px 24px 32px;">

                                        {logoSection}

                                        <h1 style="margin:0;
                                                   font-size:30px;
                                                   font-weight:bold;
                                                   color:#ffffff;
                                                   letter-spacing:0.5px;">
                                            Culinary Blog
                                        </h1>

                                        <p style="margin:12px 0 0;
                                                  color:#fbe3d2;
                                                  font-size:14px;">
                                            Nơi cảm hứng ẩm thực bắt đầu
                                        </p>
                                    </td>
                                </tr>

                                <!-- WELCOME LABEL -->
                                <tr>
                                    <td style="padding:36px 32px 0;">
                                        <p style="margin:0 0 14px;
                                                  color:#bd5b33;
                                                  font-size:12px;
                                                  font-weight:bold;
                                                  letter-spacing:1.5px;">
                                            CHÀO MỪNG THÀNH VIÊN MỚI
                                        </p>

                                        <h2 style="margin:0;
                                                   color:#33251f;
                                                   font-size:25px;
                                                   line-height:1.4;">
                                            Xin chào, {name}! 👋
                                        </h2>
                                    </td>
                                </tr>

                                <!-- INTRODUCTION -->
                                <tr>
                                    <td style="padding:18px 32px 20px;
                                               color:#53483f;
                                               font-size:15px;
                                               line-height:1.85;">

                                        <p style="margin:0 0 14px;">
                                            Chào mừng bạn đến với
                                            <strong>Culinary Blog!</strong>
                                        </p>

                                        <p style="margin:0 0 14px;">
                                            Tài khoản của bạn đã được
                                            tạo thành công.
                                        </p>

                                        <p style="margin:0;">
                                            Mỗi món ăn đều có một câu chuyện,
                                            và chúng mình rất vui khi được
                                            đồng hành cùng bạn trên hành trình
                                            khám phá những hương vị tuyệt vời.
                                        </p>
                                    </td>
                                </tr>

                                <!-- FEATURES -->
                                <tr>
                                    <td style="padding:4px 32px 24px;">

                                        <table role="presentation"
                                               width="100%"
                                               cellpadding="0"
                                               cellspacing="0"
                                               style="background:#fff5e9;
                                                      border-radius:12px;">
                                            <tr>
                                                <td style="padding:24px;
                                                           color:#53483f;
                                                           font-size:14px;
                                                           line-height:2.2;">

                                                    <p style="margin:0 0 10px;
                                                              color:#8f3d25;
                                                              font-size:16px;
                                                              font-weight:bold;">
                                                        Từ hôm nay, bạn có thể:
                                                    </p>

                                                    🍳 Khám phá những công thức
                                                    nấu ăn hấp dẫn.

                                                    <br>

                                                    ❤️ Lưu lại các món ăn
                                                    yêu thích.

                                                    <br>

                                                    ✨ Chia sẻ niềm đam mê
                                                    ẩm thực cùng cộng đồng.
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>

                                <!-- OPTIONAL ACTIVATION -->
                                {activationSection}

                                <!-- CTA -->
                                <tr>
                                    <td align="center"
                                        style="padding:4px 32px 12px;">

                                        <a href="{appUrl}"
                                           style="display:inline-block;
                                                  background:#bd5b33;
                                                  color:#ffffff;
                                                  font-size:16px;
                                                  font-weight:bold;
                                                  text-decoration:none;
                                                  padding:16px 32px;
                                                  border-radius:9px;">
                                            Khám phá Culinary Blog
                                        </a>
                                    </td>
                                </tr>

                                <!-- FALLBACK URL -->
                                <tr>
                                    <td align="center"
                                        style="padding:6px 32px 26px;
                                               font-size:12px;
                                               color:#938477;
                                               line-height:1.6;
                                               word-break:break-word;">

                                        Nếu nút không hoạt động,
                                        hãy truy cập:

                                        <br>

                                        <a href="{appUrl}"
                                           style="color:#bd5b33;
                                                  text-decoration:underline;">
                                            {appUrl}
                                        </a>
                                    </td>
                                </tr>

                                <!-- SIGNATURE -->
                                <tr>
                                    <td style="padding:0 32px 32px;
                                               color:#53483f;
                                               font-size:14px;
                                               line-height:1.9;">

                                        Chúc bạn có thật nhiều niềm vui
                                        trong căn bếp và tạo nên những
                                        món ăn đầy cảm hứng!

                                        <br><br>

                                        Thân mến,

                                        <br>

                                        <strong style="color:#8f3d25;">
                                            Đội ngũ Culinary Blog ❤️
                                        </strong>
                                    </td>
                                </tr>

                                <!-- FOOTER -->
                                <tr>
                                    <td align="center"
                                        style="background:#fcf6ef;
                                               padding:26px 24px;
                                               border-top:1px solid #f1e4d7;">

                                        <p style="margin:0 0 8px;
                                                  color:#8f3d25;
                                                  font-size:13px;
                                                  font-weight:bold;">
                                            CULINARY BLOG
                                        </p>

                                        <p style="margin:0 0 8px;
                                                  color:#948275;
                                                  font-size:12px;">
                                            Gắn kết yêu thương qua từng món ăn
                                        </p>

                                        <p style="margin:0;
                                                  color:#a8a29e;
                                                  font-size:11px;
                                                  line-height:1.7;">
                                            Đây là email được gửi tự động
                                            từ hệ thống Culinary Blog.

                                            <br>

                                            Nếu bạn không đăng ký tài khoản,
                                            bạn có thể bỏ qua email này.
                                        </p>
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

        return new WelcomeEmailContent(
            "🎉 Chào mừng bạn đến với Culinary Blog – Khởi đầu hành trình ẩm thực!",
            htmlBody);
    }
}

public sealed record WelcomeEmailContent(
    string Subject,
    string HtmlBody);
