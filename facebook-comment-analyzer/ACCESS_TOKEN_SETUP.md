# Hướng dẫn cấu hình Facebook Access Token

## 🔑 Cách lấy Facebook Access Token

### Bước 1: Tạo Facebook App
1. Truy cập [Facebook Developers](https://developers.facebook.com/)
2. Click "Create App" → "Consumer" → "Next"
3. Điền thông tin app và tạo app

### Bước 2: Cấu hình App
1. Trong app dashboard, vào "Settings" → "Basic"
2. Ghi lại **App ID** và **App Secret**
3. Thêm domain vào "App Domains" (localhost cho development)

### Bước 3: Tạo Access Token
1. Vào "Tools" → "Graph API Explorer"
2. Chọn app vừa tạo
3. Chọn permissions cần thiết:
   - `public_profile`
   - `user_posts`
   - `groups_access`
   - `pages_read_engagement`
   - `pages_show_list`
4. Click "Generate Access Token"
5. Copy token và lưu lại

## ⚙️ Cấu hình trong ứng dụng

### Cách 1: Cấu hình trong Backend (Khuyến nghị)

Chỉnh sửa file `backend/FacebookCommentAnalyzer.API/appsettings.json`:

```json
{
  "FacebookApi": {
    "BaseUrl": "https://graph.facebook.com/v18.0",
    "DefaultFields": "id,message,from,created_time,comment_count,like_count,is_hidden,can_reply",
    "AccessToken": "YOUR_ACCESS_TOKEN_HERE",
    "AppId": "YOUR_APP_ID",
    "AppSecret": "YOUR_APP_SECRET"
  }
}
```

### Cách 2: Sử dụng qua Frontend

1. Mở ứng dụng tại `http://localhost:5173`
2. Nhập Access Token vào ô "Access Token"
3. Token sẽ được gửi cùng với mỗi request

## 🔒 Bảo mật

### Lưu ý quan trọng:
- **KHÔNG** commit access token vào git
- Sử dụng environment variables cho production
- Token có thời hạn, cần refresh định kỳ
- Chỉ cấp quyền tối thiểu cần thiết

### Cách bảo vệ token trong development:

Tạo file `appsettings.Development.json` (không commit):

```json
{
  "FacebookApi": {
    "AccessToken": "YOUR_ACTUAL_TOKEN_HERE"
  }
}
```

## 📋 Quyền cần thiết cho Group Posts

Để truy cập bài viết trong Facebook groups, cần:

1. **Group phải là Public** hoặc bạn phải là member
2. **Permissions cần thiết:**
   - `public_profile` - Đọc thông tin cơ bản
   - `user_posts` - Đọc bài viết của user
   - `groups_access` - Truy cập groups
   - `pages_read_engagement` - Đọc engagement data

## 🚨 Troubleshooting

### Lỗi thường gặp:

1. **"Access token is required"**
   - Kiểm tra token đã được cấu hình chưa
   - Token có hợp lệ không

2. **"Post not found or access denied"**
   - Kiểm tra Post ID có đúng không
   - Post có phải public không
   - Token có đủ quyền không

3. **"Rate limit exceeded"**
   - Facebook có giới hạn API calls
   - Đợi một lúc rồi thử lại

4. **"Token expired"**
   - Token đã hết hạn
   - Tạo token mới

## 🔄 Refresh Token

Access Token có thể hết hạn. Để tạo token mới:

1. Vào Graph API Explorer
2. Chọn app và permissions
3. Click "Generate Access Token"
4. Cập nhật token trong cấu hình

## 📞 Hỗ trợ

Nếu gặp vấn đề:
1. Kiểm tra Facebook Developer Console
2. Xem logs trong backend
3. Kiểm tra network tab trong browser
4. Đảm bảo CORS đã được cấu hình đúng