using Application.DTOs;
using static Application.DTOs.PostDtos;
using Mapster;
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
        
        // MediaCreateDto -> Media
        TypeAdapterConfig<MediaCreateDto, Media>.NewConfig()
            .Map(dest => dest.MediaId, src => Guid.NewGuid())
            .Map(dest => dest.UploadedAt, src => DateTime.UtcNow)
            .Ignore(dest => dest.UploadedBy)
            .Ignore(dest => dest.PostId);

        // PostCreateDto -> Post
        TypeAdapterConfig<(PostCreateDto Dto, Guid UserId), Post>.NewConfig()
            .Map(dest => dest.PostId, src => Guid.NewGuid())
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.PostedAt, src => DateTime.UtcNow)
            .Map(dest => dest.IsApproved, src => true)
            .Map(dest => dest.GroupId, src => (Guid?)null)
            .Map(dest => dest.IsVisible, src => true)
            .Map(dest => dest.Content, src => src.Dto.Content)
            .Ignore(dest => dest.Media);

        // GroupPostCreateDto -> Post
        TypeAdapterConfig<(GroupPostCreateDto Dto, Guid UserId), Post>.NewConfig()
            .Map(dest => dest.PostId, src => Guid.NewGuid())
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.GroupId, src => src.Dto.GroupId)
            .Map(dest => dest.PostedAt, src => DateTime.UtcNow)
            .Map(dest => dest.IsApproved, src => false)
            .Map(dest => dest.IsVisible, src => true)
            .Map(dest => dest.Content, src => src.Dto.Content)
            .Ignore(dest => dest.Media);

        // Post -> PostDto
        TypeAdapterConfig<Post, PostDto>.NewConfig()
            .Map(dest => dest.VoteCount, src => src.PostVotes.Count(v => v.VoteType == "Vote"))
            .Map(dest => dest.IsVotedByCurrentUser, src => false)
            .Map(dest => dest.CommentCount, src => src.Comments.Count);

        // PostUpdateDto -> Post
        TypeAdapterConfig<PostUpdateDto, Post>.NewConfig()
            .Ignore(dest => dest.PostId)
            .Ignore(dest => dest.UserId)
            .Ignore(dest => dest.PostedAt)
            .Ignore(dest => dest.GroupId)
            .Ignore(dest => dest.IsApproved)
            .Ignore(dest => dest.IsVisible)
            .Ignore(dest => dest.Media);

        // GroupCreateDto -> Group
        TypeAdapterConfig<(GroupCreateDto Dto, Guid UserId), Group>.NewConfig()
            .Map(dest => dest.GroupId, src => Guid.NewGuid())
            .Map(dest => dest.CreatedBy, src => src.UserId)
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow)
            .Map(dest => dest.GroupName, src => src.Dto.GroupName)
            .Map(dest => dest.Visibility, src => src.Dto.Visibility)
            .Map(dest => dest.Image, src => src.Dto.Image);

        // Group -> GroupDto
        TypeAdapterConfig<Group, GroupDto>.NewConfig()
            .Map(dest => dest.MemberCount, src => src.GroupMembers.Count(m => m.Status == "Active"));

        // GroupMemberRequestDto -> GroupMember
        TypeAdapterConfig<(GroupMemberRequestDto Dto, Guid UserId), GroupMember>.NewConfig()
            .Map(dest => dest.GroupId, src => src.Dto.GroupId)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Role, src => "Member")
            .Map(dest => dest.JoinedAt, src => DateTime.UtcNow)
            .Map(dest => dest.Status, src => "Pending"); // Private group mặc định Pending

        // GroupMember -> GroupMemberDto
        TypeAdapterConfig<GroupMember, GroupMemberDto>.NewConfig();

        TypeAdapterConfig<Comment, CommentDto>.NewConfig();

        TypeAdapterConfig<CommentCreateDto, Comment>.NewConfig()
            .Ignore(dest => dest.CommentId)
            .Ignore(dest => dest.PostId)
            .Ignore(dest => dest.UserId)
            .Ignore(dest => dest.PostedAt);    
            
        // không được xóa ------------------------------------------------------------------------------------------------------------
        // không được xóa ------------------------------------------------------------------------------------------------------------
        // không được xóa ------------------------------------------------------------------------------------------------------------
        // Đảm bảo cấu hình được áp dụng toàn cục
        TypeAdapterConfig.GlobalSettings.Compile();
    }
}