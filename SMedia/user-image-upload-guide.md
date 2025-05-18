# User Profile Image Upload Guide

This guide explains how to upload user profile images using the existing `/api/User/{userId}` PUT endpoint, which saves the images to Azure Blob Storage.

## Prerequisites

1. Azure Storage account with a blob container
2. Proper configuration in the `.env` file:
   ```
   AZURE_STORAGE_CONNECTION_STRING=YOUR_AZURE_STORAGE_CONNECTION_STRING
   AZURE_STORAGE_CONTAINER_NAME=user-uploads
   ```

## How to Upload User Images

### Request Details

- **URL**: `/api/User/{userId}`
- **Method**: `PUT`
- **Authentication**: Required - JWT Bearer token
- **Content-Type**: `multipart/form-data`

### Request Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `userId` (in path) | string | The ID of the user whose profile is being updated |
| `imageFile` (in form) | file | Image file to upload |
| `fullName` (in form) | string | Optional - Updated full name |
| `intro` (in form) | string | Optional - Updated intro/bio |
| `birthday` (in form) | date | Optional - Updated birthday (format: YYYY-MM-DD) |
| `gender` (in form) | string | Optional - Updated gender |
| `image` (in form) | string | Optional - Direct URL to an image (used only if `imageFile` is not provided) |

### Response

```json
{
  "userId": "string",
  "username": "string",
  "fullName": "string",
  "email": "string",
  "image": "string",  // The URL of the uploaded image in Azure Blob Storage
  "intro": "string",
  "birthday": "date",
  "gender": "string",
  "joinedAt": "datetime"
}
```

### Example Usage: Frontend Implementation

#### Vanilla JavaScript

```javascript
// Function to update user profile with image
async function updateUserWithImage(userId, formData) {
  try {
    const token = localStorage.getItem('accessToken'); // Get from your auth store
    
    const response = await fetch(`/api/User/${userId}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`
      },
      body: formData // FormData containing the image and other profile data
    });
    
    if (!response.ok) {
      throw new Error(`Error updating profile: ${response.status}`);
    }
    
    const data = await response.json();
    console.log('Profile updated successfully:', data);
    return data;
    
  } catch (error) {
    console.error('Error updating profile:', error);
    throw error;
  }
}

// Usage example
const form = document.getElementById('profile-form');
form.addEventListener('submit', async (e) => {
  e.preventDefault();
  
  const userId = 'current-user-id'; // Get from auth context
  const formData = new FormData();
    // Add profile image if selected
  const imageInput = document.getElementById('profile-image');
  if (imageInput.files.length > 0) {
    formData.append('imageFile', imageInput.files[0]);
  }
  
  // Add other form fields
  formData.append('fullName', document.getElementById('full-name').value);
  formData.append('intro', document.getElementById('intro').value);
  formData.append('gender', document.getElementById('gender').value);
  
  // Birthday field (assuming it's an input type="date")
  const birthdayInput = document.getElementById('birthday');
  if (birthdayInput.value) {
    formData.append('birthday', birthdayInput.value);
  }
  
  try {
    const updatedProfile = await updateUserWithImage(userId, formData);
    // Update UI with new profile data
    document.getElementById('profile-image-preview').src = updatedProfile.image;
  } catch (error) {
    // Handle error
    alert('Failed to update profile: ' + error.message);
  }
});
```

#### React Example

```jsx
import React, { useState } from 'react';
import { useAuth } from './auth-context'; // Your auth context

function ProfileEdit() {
  const { user, accessToken } = useAuth();
  const [formData, setFormData] = useState({
    fullName: user.fullName || '',
    intro: user.intro || '',
    gender: user.gender || '',
    birthday: user.birthday || ''
  });
  const [selectedImage, setSelectedImage] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(user.image || '');
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value
    });
  };
  
  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setSelectedImage(file);
      setPreviewUrl(URL.createObjectURL(file));
    }
  };
  
  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    try {
      const submitData = new FormData();
        // Add image file if selected
      if (selectedImage) {
        submitData.append('imageFile', selectedImage);
      }
      
      // Add other form fields
      Object.keys(formData).forEach(key => {
        if (formData[key]) {
          // Use camelCase keys directly since they now match the backend expectations
          submitData.append(key, formData[key]);
        }
      });
      
      const response = await fetch(`/api/User/${user.userId}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${accessToken}`
        },
        body: submitData
      });
      
      if (!response.ok) {
        throw new Error('Failed to update profile');
      }
      
      const result = await response.json();
      
      // Update any global state with new user data
      // updateUserContext(result);
      
      alert('Profile updated successfully!');
      
    } catch (error) {
      console.error('Error updating profile:', error);
      alert(`Error: ${error.message}`);
    } finally {
      setIsSubmitting(false);
    }
  };
  
  return (
    <div className="profile-edit">
      <h2>Edit Profile</h2>
      
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Profile Image</label>
          {previewUrl && (
            <div className="image-preview">
              <img src={previewUrl} alt="Profile preview" />
            </div>
          )}
          <input
            type="file"
            accept="image/*"
            onChange={handleImageChange}
          />
        </div>
        
        <div className="form-group">
          <label>Full Name</label>
          <input
            type="text"
            name="fullName"
            value={formData.fullName}
            onChange={handleInputChange}
          />
        </div>
        
        <div className="form-group">
          <label>Bio/Intro</label>
          <textarea
            name="intro"
            value={formData.intro}
            onChange={handleInputChange}
          ></textarea>
        </div>
        
        <div className="form-group">
          <label>Gender</label>
          <select
            name="gender"
            value={formData.gender}
            onChange={handleInputChange}
          >
            <option value="">Select...</option>
            <option value="male">Male</option>
            <option value="female">Female</option>
            <option value="other">Other</option>
          </select>
        </div>
        
        <div className="form-group">
          <label>Birthday</label>
          <input
            type="date"
            name="birthday"
            value={formData.birthday}
            onChange={handleInputChange}
          />
        </div>
        
        <button 
          type="submit" 
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Updating...' : 'Save Changes'}
        </button>
      </form>
    </div>
  );
}

export default ProfileEdit;
```

## Important Notes

1. Make sure to replace `YOUR_AZURE_STORAGE_CONNECTION_STRING` in the `.env` file with your actual Azure Storage connection string
2. The image upload only happens when `ImageFile` is provided in the request
3. Images are stored in the path `user-images/{userId}_{guid}{extension}` in your Azure Blob container
4. The endpoint returns the full URL to the uploaded image, which you can use to display it in your frontend
5. Remember to include authentication in all requests - users can only update their own profiles unless they have admin role
