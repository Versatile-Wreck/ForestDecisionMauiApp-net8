// Models/DecisionRecommendation.cs
namespace ForestDecisionMauiApp.Models
{
    public enum RecommendationSeverity
    {
        Info,       // 信息
        Warning,    // 警告
        Critical,   // 严重/紧急
        Suggestion  // 建议
    }

    public class DecisionRecommendation
    {
        public string RecommendationID { get; set; } // 建议的唯一ID
        public DateTime GeneratedAt { get; set; }    // 生成时间
        public string SiteID { get; set; }           // 针对哪个监测点
        public string Basis { get; set; }            // 做出此建议的依据（例如，“速效氮低于阈值”）
        public string RecommendationText { get; set; } // 具体的建议内容
        public RecommendationSeverity Severity { get; set; } // 建议的严重性/类型
        public string RecommendedAction { get; set; } // 建议采取的行动（可选，更结构化）

        public DecisionRecommendation()
        {
            RecommendationID = "REC-" + Guid.NewGuid().ToString().Substring(0, 8);
            GeneratedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Severity}] {RecommendationText}";
        }
    }
}