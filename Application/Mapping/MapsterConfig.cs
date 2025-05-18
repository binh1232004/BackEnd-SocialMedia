using Mapster;
using Application.DTOs;
using Domain.Entities;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        // Đăng ký cấu hình ánh xạ cho RegisterDto -> user
        TypeAdapterConfig<RegisterDto, User>.NewConfig()
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.Birthday, src => src.Birthday)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.UserId, src => Guid.NewGuid().ToString())
            .Map(dest => dest.DeletedUserEmail, src => (string?)null)
            .Map(dest => dest.JoinedAt, src => DateTime.UtcNow)
            .Map(dest => dest.Status, src => "active")
            .Map(dest => dest.Intro, src => (string?)null)
            .Map(dest => dest.Image, src => "https://res.cloudinary.com/dapvvdxw7/image/upload/v1747159636/avatar_l2rwth.jpg")
            .Ignore(dest => dest.PasswordHash)
            .AfterMapping((src, dest) => dest.SetPassword(src.Password));
        
        // Ánh xạ cho MessageDto -> message
        // TypeAdapterConfig<MessageDto, message>.NewConfig()
        //     .Map(dest => dest.message_id, src => src.message_id ?? Guid.NewGuid().ToString()) // Tạo mới message_id nếu null
        //     .Map(dest => dest.sender_id, src => src.sender_id)
        //     .Map(dest => dest.receiver_id, src => src.receiver_id)
        //     .Map(dest => dest.group_chat_id, src => src.group_chat_id)
        //     .Map(dest => dest.content, src => src.content)
        //     .Map(dest => dest.media_type, src => src.media_type)
        //     .Map(dest => dest.media_url, src => src.media_url)
        //     .Map(dest => dest.sent_time, src => src.sent_time ?? DateTime.UtcNow) // Gán mặc định nếu null
        //     .Map(dest => dest.is_read, src => src.is_read ?? false); // Gán mặc định nếu null
        //
        // // Ánh xạ cho message -> MessageDto
        // TypeAdapterConfig<message, MessageDto>.NewConfig()
        //     .Map(dest => dest.message_id, src => src.message_id)
        //     .Map(dest => dest.sender_id, src => src.sender_id)
        //     .Map(dest => dest.receiver_id, src => src.receiver_id)
        //     .Map(dest => dest.group_chat_id, src => src.group_chat_id)
        //     .Map(dest => dest.content, src => src.content)
        //     .Map(dest => dest.media_type, src => src.media_type)
        //     .Map(dest => dest.media_url, src => src.media_url)
        //     .Map(dest => dest.sent_time, src => src.sent_time)
        //     .Map(dest => dest.is_read, src => src.is_read);

        // Đảm bảo cấu hình được áp dụng toàn cục
        TypeAdapterConfig.GlobalSettings.Compile();
    }
}