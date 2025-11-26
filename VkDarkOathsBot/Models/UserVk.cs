using DarkOathsAspireBackendToReact.AuthService.Data;

namespace VkDarkOathsBot.Models
{
    public class UserVk
    {
        /// <summary>
        /// Id.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// VkId из приложения, для работы бота.
        /// </summary>
        public string VkId { get; set; } = string.Empty;

        /// <summary>
        /// Связь с таблицей Users.
        /// </summary>
        public Guid? LinkedAuthUserId { get; set; }

        /// <summary>
        /// Уникальный код для команды /register.
        /// </summary>
        public string LinkingCode { get; set; } = string.Empty; 

        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
