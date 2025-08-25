# 🕷️ Facebook Web Scraping Guide

## Tổng Quan

Hệ thống web scraping mới cho phép bạn:
- ✅ Nhập URL Facebook post trực tiếp (không cần API token)
- ✅ Scrape toàn bộ comments từ posts public
- ✅ Lấy thông tin user: tên, link profile, thời gian comment
- ✅ Kiểm tra ai đã share post đó
- ✅ Filter và sort comments theo nhiều tiêu chí
- ✅ Export dữ liệu ra JSON/CSV

## 🔄 So Sánh Hai Phương Pháp

| Tính Năng | Graph API (Cũ) | Web Scraping (Mới) |
|-----------|----------------|---------------------|
| **Yêu cầu token** | ✅ Cần access token | ❌ Không cần |
| **Giới hạn data** | ⚠️ Chỉ posts mà user có quyền | ✅ Tất cả posts public |
| **Tốc độ** | ⚡ Nhanh | 🐌 Chậm hơn (1-3 phút) |
| **Độ tin cậy** | ✅ Cao | ⚠️ Phụ thuộc DOM Facebook |
| **Setup** | 🔧 Phức tạp | ✅ Đơn giản |

## 🚀 Cách Sử Dụng

### 1. Setup Environment

```bash
# Backend - cài Selenium dependencies
cd facebook-comment-analyzer/backend/FacebookCommentAnalyzer.API
dotnet restore

# Frontend - không cần thay đổi
cd facebook-comment-analyzer/frontend/facebook-comment-frontend
npm install
```

### 2. Chạy Ứng Dụng

```bash
# Terminal 1 - Backend
cd facebook-comment-analyzer/backend/FacebookCommentAnalyzer.API
dotnet run

# Terminal 2 - Frontend  
cd facebook-comment-analyzer/frontend/facebook-comment-frontend
npm run dev
```

### 3. Sử Dụng Web Scraping

1. Mở http://localhost:5173
2. Click "🕷️ Web Scraping" để chuyển sang chế độ mới
3. Paste URL Facebook post vào ô input
4. Cấu hình options (số comments, sort order, filters)
5. Click "🕷️ Start Scraping"

## 📝 Supported URL Formats

Hệ thống hỗ trợ các định dạng URL sau:

```
# Group Posts
https://facebook.com/groups/123456789/posts/987654321
https://facebook.com/groups/groupname/posts/987654321
https://facebook.com/groups/123456789/permalink/987654321

# Page Posts  
https://facebook.com/pagename/posts/987654321

# Profile Posts
https://facebook.com/profile.php?id=123456789&story_fbid=987654321

# Mobile URLs (sẽ được convert tự động)
https://m.facebook.com/groups/123456789/posts/987654321
```

## ⚙️ API Endpoints

### Web Scraping API

```http
POST /api/webscraping/scrape-post-comments
Content-Type: application/json

{
  "postUrl": "https://facebook.com/groups/123/posts/456",
  "maxComments": 1000,
  "sortOrder": "OldestFirst",
  "loadReplies": false,
  "loadReactions": false,
  "filters": {
    "startDate": "2024-01-01",
    "endDate": "2024-12-31",
    "minLikes": 5,
    "onlySharers": true,
    "contentKeywords": "keyword1, keyword2"
  }
}
```

### Validate URL

```http
POST /api/webscraping/validate-url
Content-Type: application/json

{
  "url": "https://facebook.com/groups/123/posts/456"
}
```

### Get Post Info Only

```http
POST /api/webscraping/get-post-info
Content-Type: application/json

{
  "postUrl": "https://facebook.com/groups/123/posts/456"  
}
```

### Get Sharers Only

```http
POST /api/webscraping/get-sharers
Content-Type: application/json

{
  "postUrl": "https://facebook.com/groups/123/posts/456",
  "maxComments": 1000
}
```

## 📊 Response Format

### Scraped Comment Data

```json
{
  "success": true,
  "data": {
    "postInfo": {
      "postId": "123456789",
      "content": "Nội dung post...",
      "authorName": "Tên tác giả",
      "authorProfileUrl": "https://facebook.com/profile",
      "postTime": "2024-01-15T10:30:00Z",
      "likesCount": 150,
      "commentsCount": 85,
      "sharesCount": 25,
      "groupName": "Tên group",
      "isGroupPost": true
    },
    "comments": [
      {
        "commentId": "987654321",
        "content": "Nội dung comment...",
        "authorName": "Người comment",
        "authorProfileUrl": "https://facebook.com/commenter",
        "commentTime": "2024-01-15T11:00:00Z",
        "likesCount": 5,
        "repliesCount": 2,
        "hasSharedPost": true,
        "shareUrl": "https://facebook.com/share/link",
        "shareTime": "2024-01-15T12:00:00Z",
        "isGroupMember": true,
        "groupRole": "member"
      }
    ],
    "totalCommentsScraped": 85,
    "sharersCount": 12,
    "groupMembersCount": 78
  }
}
```

## 🔍 Advanced Filtering

Hệ thống hỗ trợ filter theo:

- **Thời gian**: từ ngày - đến ngày
- **Engagement**: số likes tối thiểu  
- **Share status**: chỉ lấy người đã share
- **Group membership**: chỉ thành viên group
- **Keywords**: tìm theo từ khóa trong nội dung
- **Author names**: tìm theo tên tác giả

## 📤 Export Options

- **JSON**: Xuất toàn bộ data với metadata
- **CSV**: Xuất dạng bảng cho Excel/Google Sheets
- **Sharers Only**: Chỉ xuất danh sách người share

## ⚠️ Lưu Ý Quan Trọng

### Browser Requirements
- Chrome/Chromium phải được cài đặt trên server
- ChromeDriver tương thích với version Chrome

### Performance
- Scraping 1000 comments: ~2-3 phút
- Sử dụng headless browser để tăng tốc độ
- Rate limiting để tránh bị block

### Limitations
- Chỉ hoạt động với posts/groups PUBLIC
- Facebook có thể thay đổi DOM structure
- Có thể bị rate limit nếu scrape quá nhiều

### Privacy & Legal
- Chỉ scrape dữ liệu công khai
- Tuân thủ Terms of Service của Facebook
- Không lưu trữ thông tin cá nhân nhạy cảm

## 🛠️ Troubleshooting

### Chrome Driver Issues
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install google-chrome-stable

# CentOS/RHEL
sudo yum install google-chrome-stable
```

### Memory Issues
```bash
# Tăng memory limit cho container
export CHROME_OPTS="--no-sandbox --disable-dev-shm-usage --disable-gpu"
```

### Facebook Blocking
- Sử dụng proxy servers
- Rotate user agents
- Thêm random delays
- Sử dụng residential IP

## 🔧 Configuration

### Backend Settings (appsettings.json)
```json
{
  "WebScraping": {
    "ChromeOptions": {
      "Headless": true,
      "NoSandbox": true,
      "DisableGpu": true,
      "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
    },
    "DefaultTimeout": 60000,
    "MaxScrollAttempts": 50,
    "ScrollDelayMs": 2000
  }
}
```

### Frontend Environment
```bash
# Development
VITE_API_BASE_URL=http://localhost:5000

# Production  
VITE_API_BASE_URL=https://your-api-domain.com
```

## 📈 Monitoring & Logs

Hệ thống ghi log chi tiết cho:
- URL validation
- Scraping progress  
- Errors và exceptions
- Performance metrics

Check logs trong:
- Backend: Console output
- Browser: Selenium logs
- API: ILogger output

## 🔮 Future Enhancements

- [ ] Support Instagram/TikTok
- [ ] Real-time scraping với WebSocket
- [ ] Machine learning cho sentiment analysis
- [ ] Notification system
- [ ] Automated scheduling
- [ ] Advanced analytics dashboard