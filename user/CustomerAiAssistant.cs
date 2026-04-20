using System;

namespace WinFormsApp
{
    internal static class CustomerAiAssistant
    {
        public static AiSupportResult Analyze(string input)
        {
            var normalized = (input ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return new AiSupportResult(
                    "Chưa xác định",
                    "Trung bình",
                    "Mô tả ngắn tình huống bạn đang gặp để AI đề xuất hướng xử lý.",
                    "Gõ địa điểm, dấu hiệu nhận dạng, số người liên quan và mức khẩn cấp.",
                    false);
            }

            if (ContainsAny(normalized, "cuop", "cướp", "dao", "sung", "súng", "danh", "đánh", "hanh hung", "hành hung"))
            {
                return new AiSupportResult(
                    "Nguy cơ bạo lực",
                    "Khẩn cấp",
                    "Rời khỏi khu vực nguy hiểm nếu có thể, giữ khoảng cách an toàn và ưu tiên gọi 113 ngay.",
                    "Không tự đối đầu. Ghi nhớ đặc điểm nhận dạng và chia sẻ vị trí hiện tại cho lực lượng hỗ trợ.",
                    true);
            }

            if (ContainsAny(normalized, "mat cap", "mất cắp", "trom", "trộm", "giat", "giật", "xe bi lay", "xe bị lấy"))
            {
                return new AiSupportResult(
                    "Mất cắp tài sản",
                    "Cao",
                    "Kiểm tra lần cuối vị trí tài sản, chụp lại hiện trường và khóa các tài khoản liên quan nếu có.",
                    "Chuẩn bị biển số, thời gian xảy ra, camera gần nhất và người chứng kiến để gửi báo cáo nhanh.",
                    false);
            }

            if (ContainsAny(normalized, "lua dao", "lừa đảo", "chuyen khoan", "chuyển khoản", "gia mao", "giả mạo", "otp"))
            {
                return new AiSupportResult(
                    "Nghi ngờ lừa đảo",
                    "Cao",
                    "Ngưng chuyển tiền, khóa tài khoản ngân hàng hoặc ví điện tử nếu vừa cung cấp thông tin nhạy cảm.",
                    "Lưu lại số điện thoại, tài khoản nhận tiền, tin nhắn và ảnh chụp màn hình để cơ quan chức năng xác minh.",
                    false);
            }

            if (ContainsAny(normalized, "tai nan", "tai nạn", "va cham", "va chạm", "thuong", "thương", "chay", "cháy"))
            {
                return new AiSupportResult(
                    "Sự cố hiện trường",
                    "Khẩn cấp",
                    "Đảm bảo an toàn cho người bị nạn trước, tránh tụ tập giữa đường và gọi hỗ trợ khẩn cấp nếu có người bị thương.",
                    "Cung cấp mốc vị trí, số người liên quan và tình trạng giao thông để đội phản ứng tiếp cận nhanh hơn.",
                    true);
            }

            if (ContainsAny(normalized, "gay roi", "gây rối", "on ao", "ồn ào", "khua khoan", "khuya", "tu tap", "tụ tập"))
            {
                return new AiSupportResult(
                    "Mất trật tự công cộng",
                    "Trung bình",
                    "Giữ khoảng cách quan sát an toàn, tránh quay cận mặt nếu tình hình đang căng thẳng.",
                    "Ghi nhận số người, phương tiện và thời điểm để gửi báo cáo chính xác hơn.",
                    false);
            }

            return new AiSupportResult(
                "Tình huống cần xác minh",
                "Trung bình",
                "AI chưa nhận diện rõ loại vụ việc nhưng bạn vẫn nên gửi mô tả ngắn gọn kèm vị trí và thời gian xảy ra.",
                "Nếu có dấu hiệu đe dọa trực tiếp đến tính mạng, hãy gọi 113 ngay thay vì chờ phản hồi trong ứng dụng.",
                false);
        }

        private static bool ContainsAny(string input, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal sealed record AiSupportResult(
        string Category,
        string Priority,
        string Guidance,
        string RecommendedAction,
        bool ShouldCallEmergency);
}
