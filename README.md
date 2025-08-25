# 🚀 Facebook Comment Analyzer Tool

> **Công cụ phân tích comment Facebook mạnh mẽ** - Lấy dữ liệu từ bài viết Facebook, phân tích comment, kiểm tra share activity và theo dõi engagement của người dùng.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-.NET%208%20%7C%20Vue.js%203-blue)

## 📋 Mục lục

- [🎯 Tính năng chính](#-tính-năng-chính)
- [🛠️ Yêu cầu hệ thống](#️-yêu-cầu-hệ-thống)
- [⚡ Cài đặt nhanh](#-cài-đặt-nhanh)
- [🔧 Cài đặt chi tiết](#-cài-đặt-chi-tiết)
- [🔑 Cấu hình Facebook Access Token](#-cấu-hình-facebook-access-token)
- [🚀 Chạy ứng dụng](#-chạy-ứng-dụng)
- [📖 Hướng dẫn sử dụng](#-hướng-dẫn-sử-dụng)
- [🔍 API Documentation](#-api-documentation)
- [🛠️ Troubleshooting](#️-troubleshooting)
- [🤝 Đóng góp](#-đóng-góp)

## 🎯 Tính năng chính

### ✨ Core Features
- 📊 **Phân tích bài viết Facebook** - Hỗ trợ cả regular posts và group posts
- 💬 **Thu thập comment** - Lấy tất cả comment với pagination
- 🔍 **Tìm kiếm thông minh** - Tìm kiếm theo nội dung, tên người dùng
- 📅 **Sắp xếp linh hoạt** - Theo thời gian, likes, share status, group role
- ✅ **Kiểm tra share activity** - Phân tích chi tiết việc share bài viết
- 🏷️ **Group analytics** - Thông tin membership và vai trò trong group

### 🎨 UI/UX Features
- 🎨 **Giao diện hiện đại** - Responsive design với Vue.js 3
- 🔄 **Real-time updates** - Cập nhật dữ liệu theo thời gian thực
- 📱 **Mobile friendly** - Tối ưu cho mọi thiết bị
- 🎯 **User experience** - Dễ sử dụng, trực quan

### 🔧 Technical Features
- 🔐 **Bảo mật cao** - Access token management an toàn
- ⚡ **Performance** - Tối ưu tốc độ xử lý
- 🔄 **CORS support** - Cross-origin resource sharing
- 📊 **Error handling** - Xử lý lỗi thông minh

## 🛠️ Yêu cầu hệ thống

### Backend Requirements
- **.NET 8.0 SDK** hoặc cao hơn
- **Windows 10/11, macOS 10.15+, Ubuntu 18.04+**
- **RAM**: Tối thiểu 2GB
- **Disk space**: 500MB trống

### Frontend Requirements  
- **Node.js 16.0+** và npm
- **Modern browser** (Chrome 90+, Firefox 88+, Safari 14+)
- **RAM**: Tối thiểu 1GB
- **Internet connection** để tải dependencies

### Facebook Requirements
- **Facebook Developer Account**
- **Facebook App** với permissions phù hợp
- **Access Token** với quyền truy cập cần thiết

## ⚡ Cài đặt nhanh

### 1. Clone repository
```bash
git clone https://github.com/your-username/facebook-comment-analyzer.git
cd facebook-comment-analyzer
```

### 2. Cài đặt dependencies
```bash
# Backend
cd backend/FacebookCommentAnalyzer.API
dotnet restore

# Frontend
cd ../../frontend/facebook-comment-frontend
npm install
```

### 3. Cấu hình Access Token
```bash
# Copy file cấu hình
cp appsettings.json appsettings.Development.json
# Chỉnh sửa appsettings.Development.json với token thực
```

### 4. Chạy ứng dụng
```bash
# Terminal 1 - Backend
cd backend/FacebookCommentAnalyzer.API
dotnet run

# Terminal 2 - Frontend  
cd frontend/facebook-comment-frontend
npm run dev
```

### 5. Truy cập ứng dụng
- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5000

## 🔧 Cài đặt chi tiết

### Bước 1: Chuẩn bị môi trường

#### Windows
```bash
# Cài đặt .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8

# Cài đặt Node.js
winget install OpenJS.NodeJS
```

#### macOS
```bash
# Sử dụng Homebrew
brew install dotnet
brew install node

# Hoặc sử dụng nvm
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash
nvm install 18
nvm use 18
```

#### Ubuntu/Debian
```bash
# Cài đặt .NET 8.0
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Cài đặt Node.js
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs
```

### Bước 2: Clone và setup project

```bash
# Clone repository
git clone https://github.com/your-username/facebook-comment-analyzer.git
cd facebook-comment-analyzer

# Kiểm tra cấu trúc project
ls -la
```

### Bước 3: Cài đặt Backend

```bash
# Di chuyển vào thư mục backend
cd backend/FacebookCommentAnalyzer.API

# Kiểm tra .NET version
dotnet --version

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Kiểm tra build thành công
dotnet run --no-build --help
```

### Bước 4: Cài đặt Frontend

```bash
# Di chuyển vào thư mục frontend
cd ../../frontend/facebook-comment-frontend

# Kiểm tra Node.js version
node --version
npm --version

# Cài đặt dependencies
npm install

# Kiểm tra cài đặt thành công
npm run build
```

## 🔑 Cấu hình Facebook Access Token

### Bước 1: Tạo Facebook App

1. **Truy cập Facebook Developers**
   ```
   https://developers.facebook.com/
   ```

2. **Tạo App mới**
   - Click "Create App"
   - Chọn "Consumer" → "Next"
   - Điền thông tin app:
     - App Name: `Facebook Comment Analyzer`
     - App Contact Email: `your-email@example.com`
     - Business Account: (Optional)

3. **Cấu hình App Settings**
   - Vào "Settings" → "Basic"
   - Ghi lại **App ID** và **App Secret**
   - Thêm domain: `localhost`

### Bước 2: Tạo Access Token

1. **Vào Graph API Explorer**
   ```
   https://developers.facebook.com/tools/explorer/
   ```

2. **Chọn App và Permissions**
   - App: Chọn app vừa tạo
   - Permissions cần thiết:
     ```
     public_profile
     user_posts
     groups_access
     pages_read_engagement
     pages_show_list
     ```

3. **Generate Token**
   - Click "Generate Access Token"
   - Copy token và lưu lại an toàn

### Bước 3: Cấu hình trong ứng dụng

#### Cách 1: Cấu hình Backend (Khuyến nghị)

```bash
# Tạo file cấu hình development
cd backend/FacebookCommentAnalyzer.API
cp appsettings.json appsettings.Development.json
```

Chỉnh sửa `appsettings.Development.json`:
```json
{
  "FacebookApi": {
    "BaseUrl": "https://graph.facebook.com/v18.0",
    "DefaultFields": "id,message,from,created_time,comment_count,like_count,is_hidden,can_reply",
    "AccessToken": "YOUR_ACTUAL_ACCESS_TOKEN_HERE",
    "AppId": "YOUR_APP_ID",
    "AppSecret": "YOUR_APP_SECRET"
  }
}
```

#### Cách 2: Sử dụng Environment Variables

```bash
# Windows
set FACEBOOK_ACCESS_TOKEN=your_token_here

# macOS/Linux
export FACEBOOK_ACCESS_TOKEN=your_token_here
```

## 🚀 Chạy ứng dụng

### Chạy Backend

```bash
# Terminal 1
cd backend/FacebookCommentAnalyzer.API

# Chạy trong development mode
dotnet run

# Hoặc chạy với environment cụ thể
dotnet run --environment Development

# Kiểm tra API hoạt động
curl http://localhost:5000/api/facebook/config
```

**Backend sẽ chạy tại**: http://localhost:5000

### Chạy Frontend

```bash
# Terminal 2
cd frontend/facebook-comment-frontend

# Chạy development server
npm run dev

# Hoặc chạy với host cụ thể
npm run dev -- --host 0.0.0.0
```

**Frontend sẽ chạy tại**: http://localhost:5173

### Kiểm tra hoạt động

1. **Mở browser** và truy cập http://localhost:5173
2. **Kiểm tra API status** trong phần "API Configuration Status"
3. **Test với Post ID** thực tế từ Facebook

## 📖 Hướng dẫn sử dụng

### 1. Phân tích Regular Post

1. **Chọn loại post**: "Regular Post"
2. **Nhập Post ID**: Lấy từ URL Facebook post
   ```
   Ví dụ: https://www.facebook.com/username/posts/123456789_987654321
   Post ID: 123456789_987654321
   ```
3. **Nhập Access Token** (nếu chưa cấu hình trong backend)
4. **Click "Get Post Info"** để xem thông tin bài viết
5. **Click "Analyze Comments"** để phân tích comment

### 2. Phân tích Group Post

1. **Chọn loại post**: "Group Post"
2. **Nhập Group Post ID**: Lấy từ URL group post
   ```
   Ví dụ: https://www.facebook.com/groups/groupname/posts/123456789_987654321
   Post ID: 123456789_987654321
   ```
3. **Thực hiện phân tích** như regular post
4. **Xem thông tin group**: Membership, role, etc.

### 3. Sử dụng Filters và Search

#### Tìm kiếm
- **Search box**: Tìm theo nội dung comment hoặc tên người dùng
- **Real-time**: Kết quả cập nhật ngay lập tức

#### Sắp xếp
- **Sort by Date**: Từ cũ đến mới
- **Sort by Likes**: Theo số like
- **Sort by Shared**: Người share trước
- **Sort by Group Role**: Admin → Moderator → Member

#### Lọc
- **All Comments**: Tất cả comment
- **Shared Post**: Chỉ người đã share
- **Not Shared**: Chỉ người chưa share
- **Group Members**: Chỉ thành viên group

### 4. Xem thông tin chi tiết

#### Comment Card
- **Avatar và tên**: Người comment
- **Thời gian**: Khi comment
- **Nội dung**: Comment message
- **Badges**: Shared Post, Group Member, Role
- **Stats**: Likes, Replies

#### Share Analysis
- **Share Message**: Nội dung khi share
- **Share Type**: Public, Friends, Private
- **Share Time**: Thời gian share
- **Share Stats**: Likes, comments trên share

## 🔍 API Documentation

### Base URL
```
http://localhost:5000/api/facebook
```

### Authentication
```http
GET /api/facebook/config
Authorization: Not required
```

### Regular Posts

#### Get Post Info
```http
GET /api/facebook/post/{postId}?access_token={token}
```

#### Get Comments
```http
GET /api/facebook/post/{postId}/comments?access_token={token}
```

#### Analyze Comments
```http
GET /api/facebook/post/{postId}/analyze?access_token={token}
```

### Group Posts

#### Get Group Post Info
```http
GET /api/facebook/group-post/{postId}?access_token={token}
```

#### Get Group Comments
```http
GET /api/facebook/group-post/{postId}/comments?access_token={token}
```

#### Analyze Group Comments
```http
GET /api/facebook/group-post/{postId}/analyze?access_token={token}
```

### User Analysis

#### Check User Share
```http
GET /api/facebook/user/{userId}/check-share?postUrl={url}&access_token={token}
```

#### Detailed Share Analysis
```http
GET /api/facebook/user/{userId}/share-analysis?postUrl={url}&access_token={token}
```

#### Group Member Info
```http
GET /api/facebook/group/{groupId}/member/{userId}?access_token={token}
```

### Response Examples

#### Post Info Response
```json
{
  "id": "123456789_987654321",
  "message": "Post content here...",
  "created_time": "2024-01-01T12:00:00+0000",
  "from": {
    "id": "123456789",
    "name": "User Name",
    "picture": {
      "data": {
        "url": "https://graph.facebook.com/..."
      }
    }
  },
  "likes": {
    "data": [...]
  },
  "shares": 5
}
```

#### Comment Analysis Response
```json
[
  {
    "id": "comment_id",
    "message": "Comment content",
    "from": {
      "id": "user_id",
      "name": "User Name"
    },
    "created_time": "2024-01-01T12:30:00+0000",
    "like_count": 10,
    "comment_count": 2,
    "hasSharedPost": true,
    "shareUrl": "https://facebook.com/...",
    "shareType": "public",
    "shareMessage": "Shared message",
    "shareTime": "2024-01-01T13:00:00+0000",
    "isGroupMember": true,
    "groupRole": "member"
  }
]
```

## 🛠️ Troubleshooting

### Lỗi thường gặp

#### 1. "Access token is required"
```bash
# Kiểm tra cấu hình
cat backend/FacebookCommentAnalyzer.API/appsettings.Development.json

# Hoặc set environment variable
export FACEBOOK_ACCESS_TOKEN=your_token_here
```

#### 2. "Post not found or access denied"
- ✅ Kiểm tra Post ID có đúng không
- ✅ Post có phải public không
- ✅ Token có đủ quyền không
- ✅ Group có phải public hoặc bạn là member không

#### 3. "Rate limit exceeded"
```bash
# Đợi 15-30 phút rồi thử lại
# Hoặc sử dụng token khác
```

#### 4. "Token expired"
```bash
# Tạo token mới tại Graph API Explorer
# Cập nhật trong appsettings.Development.json
```

#### 5. Backend không start
```bash
# Kiểm tra .NET version
dotnet --version

# Clean và rebuild
dotnet clean
dotnet restore
dotnet build
dotnet run
```

#### 6. Frontend không start
```bash
# Kiểm tra Node.js version
node --version

# Clean node_modules
rm -rf node_modules package-lock.json
npm install
npm run dev
```

#### 7. CORS errors
```bash
# Kiểm tra CORS config trong Program.cs
# Đảm bảo frontend URL được allow
```

### Debug Mode

#### Backend Debug
```bash
# Chạy với logging chi tiết
dotnet run --environment Development --verbosity detailed

# Xem logs
tail -f logs/app.log
```

#### Frontend Debug
```bash
# Chạy với debug mode
npm run dev -- --debug

# Mở browser dev tools
# Xem Console và Network tabs
```

### Performance Issues

#### Backend Performance
```bash
# Monitor memory usage
dotnet-counters monitor

# Profile performance
dotnet-trace collect --name facebook-analyzer
```

#### Frontend Performance
```bash
# Build production version
npm run build

# Analyze bundle size
npm run analyze
```

## 🤝 Đóng góp

### Cách đóng góp

1. **Fork repository**
   ```bash
   git clone https://github.com/your-username/facebook-comment-analyzer.git
   ```

2. **Tạo feature branch**
   ```bash
   git checkout -b feature/amazing-feature
   ```

3. **Commit changes**
   ```bash
   git add .
   git commit -m "Add amazing feature"
   ```

4. **Push to branch**
   ```bash
   git push origin feature/amazing-feature
   ```

5. **Mở Pull Request**

### Development Guidelines

#### Code Style
- **Backend**: Follow C# conventions
- **Frontend**: Use ESLint + Prettier
- **Commits**: Use conventional commits

#### Testing
```bash
# Backend tests
dotnet test

# Frontend tests
npm run test
```

#### Documentation
- Update README.md
- Add API documentation
- Include examples

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Facebook Graph API
- .NET Core team
- Vue.js team
- Open source community

## 📞 Support

### Getting Help

1. **Check Documentation**: Read this README thoroughly
2. **Search Issues**: Look for similar problems
3. **Create Issue**: Provide detailed information
4. **Community**: Ask in discussions

### Issue Template

When creating an issue, include:

```markdown
**Environment:**
- OS: [Windows/macOS/Linux]
- .NET Version: [8.0.x]
- Node.js Version: [18.x.x]
- Browser: [Chrome/Firefox/Safari]

**Problem:**
[Describe the issue]

**Steps to reproduce:**
1. [Step 1]
2. [Step 2]
3. [Step 3]

**Expected behavior:**
[What should happen]

**Actual behavior:**
[What actually happens]

**Logs:**
[Include relevant logs]
```

---

**Made with ❤️ by the Facebook Comment Analyzer Team**

[![GitHub stars](https://img.shields.io/github/stars/your-username/facebook-comment-analyzer?style=social)](https://github.com/your-username/facebook-comment-analyzer)
[![GitHub forks](https://img.shields.io/github/forks/your-username/facebook-comment-analyzer?style=social)](https://github.com/your-username/facebook-comment-analyzer)
[![GitHub issues](https://img.shields.io/github/issues/your-username/facebook-comment-analyzer)](https://github.com/your-username/facebook-comment-analyzer/issues)