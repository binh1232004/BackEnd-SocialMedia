# API Group Posts - Update Visibility

This endpoint allows group administrators to toggle the visibility of a post within a group.

## Endpoint

```
PUT /api/group-posts/visible
```

## Authorization

- Bearer token required
- User must be an administrator of the group to which the post belongs

## Request Body

```json
{
  "postId": "string (Guid)",
  "isVisible": boolean
}
```

### Fields

- `postId`: The ID of the post to update
- `isVisible`: The new visibility state (true = visible, false = hidden)

## Responses

### 200 OK

The visibility was successfully updated.

```json
{
  "postId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "content": "string",
  "postedAt": "2023-06-07T12:34:56.789Z",
  "groupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isApproved": true,
  "isVisible": true,
  "media": [
    {
      "mediaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "mediaUrl": "string",
      "mediaType": "string",
      "uploadedAt": "2023-06-07T12:34:56.789Z",
      "uploadedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ],
  "voteCount": 0,
  "isVotedByCurrentUser": false,
  "commentCount": 0
}
```

### 400 Bad Request

Either the post is not a group post, or its visibility state is already set to the requested value.

```json
{
  "error": "string"
}
```

### 401 Unauthorized

The user is not authorized to update the post visibility (not an admin of the group).

```json
{
  "error": "string"
}
```

### 404 Not Found

The specified post was not found.

```json
{
  "error": "string"
}
```

### 500 Internal Server Error

An unexpected error occurred on the server.

```json
{
  "error": "string"
}
```

## Example

### Request

```http
PUT /api/group-posts/visible HTTP/1.1
Host: api.example.com
Authorization: Bearer <token>
Content-Type: application/json

{
  "postId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isVisible": true
}
```

### Response

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "postId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "content": "This is a sample post content",
  "postedAt": "2023-06-07T12:34:56.789Z",
  "groupId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isApproved": true,
  "isVisible": true,
  "media": [
    {
      "mediaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "mediaUrl": "https://example.com/images/sample.jpg",
      "mediaType": "image/jpeg",
      "uploadedAt": "2023-06-07T12:34:56.789Z",
      "uploadedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ],
  "voteCount": 5,
  "isVotedByCurrentUser": false,
  "commentCount": 3
}
```

## Notes

- Only group administrators can change post visibility
- Changing visibility to `false` will hide the post from regular group members, but it will still be visible to administrators
- Posts can be made visible or hidden regardless of their approval status
