# Facebook Comment Scraper

Ứng dụng web scraping để lấy và phân tích comment từ bài viết Facebook public, bao gồm kiểm tra xem người comment có share bài viết hay không.

## 🚀 Tính năng

- **Web Scraping**: Sử dụng Puppeteer để scrape dữ liệu từ Facebook
- **Comment Analysis**: Lấy toàn bộ comment với thông tin chi tiết
- **User Information**: Tên người dùng, link profile, thời gian comment
- **Share Detection**: Kiểm tra xem user có share bài viết hay không
- **Sorting & Filtering**: Sắp xếp comment theo thời gian, lọc theo trạng thái share
- **Search**: Tìm kiếm comment theo nội dung hoặc tên người dùng
- **Modern UI**: Giao diện đẹp và responsive

## 📋 Yêu cầu hệ thống

- Node.js 16+ 
- npm hoặc yarn
- Chrome/Chromium browser (cho Puppeteer)

## 🛠️ Cài đặt

1. **Clone repository**:
```bash
git clone <repository-url>
cd facebook-scraper-server
```

2. **Cài đặt dependencies**:
```bash
npm install
```

3. **Chạy ứng dụng**:
```bash
# Development mode
npm run dev

# Production mode
npm start
```

4. **Truy cập ứng dụng**:
```
http://localhost:3000
```

## 🎯 Cách sử dụng

### 1. Nhập URL Facebook Post
- Mở trình duyệt và truy cập `http://localhost:3000`
- Nhập URL bài viết Facebook public vào ô input
- Click "Bắt đầu Scrape"

### 2. Xem kết quả
- Hệ thống sẽ hiển thị tổng số comment
- Danh sách comment với thông tin chi tiết
- Thời gian comment được sắp xếp từ cũ nhất đến mới nhất

### 3. Kiểm tra Share
- Click "Kiểm tra Share" cho từng comment
- Hệ thống sẽ kiểm tra xem user có share bài viết hay không

### 4. Tìm kiếm và lọc
- Sử dụng thanh tìm kiếm để tìm comment
- Lọc comment theo trạng thái share

## 🔧 API Endpoints

### POST `/api/scrape`
Scrape comment từ Facebook post

**Request Body:**
```json
{
  "postUrl": "https://www.facebook.com/username/posts/123456789"
}
```

**Response:**
```json
{
  "success": true,
  "postUrl": "https://www.facebook.com/username/posts/123456789",
  "totalComments": 50,
  "comments": [
    {
      "id": "comment_id",
      "text": "Nội dung comment",
      "userName": "Tên người dùng",
      "userProfile": "https://facebook.com/user",
      "timestamp": "2 giờ trước",
      "createdAt": "2024-01-01T10:00:00.000Z"
    }
  ]
}
```

### POST `/api/check-share`
Kiểm tra user có share bài viết hay không

**Request Body:**
```json
{
  "postUrl": "https://www.facebook.com/username/posts/123456789",
  "userProfile": "https://facebook.com/user"
}
```

**Response:**
```json
{
  "hasShared": true
}
```

### GET `/api/health`
Health check endpoint

**Response:**
```json
{
  "status": "OK",
  "timestamp": "2024-01-01T10:00:00.000Z"
}
```

## 🏗️ Cấu trúc dự án

```
facebook-scraper-server/
├── server.js              # Server chính
├── package.json           # Dependencies
├── public/
│   └── index.html         # Giao diện web
└── README.md              # Hướng dẫn
```

## ⚠️ Lưu ý quan trọng

### Facebook Scraping Limitations
- **Public Posts Only**: Chỉ có thể scrape bài viết public
- **Rate Limiting**: Facebook có thể giới hạn request
- **Detection**: Facebook có thể phát hiện và chặn bot
- **Terms of Service**: Tuân thủ điều khoản sử dụng Facebook

### Best Practices
- Sử dụng với tần suất hợp lý
- Không scrape quá nhiều bài viết cùng lúc
- Tôn trọng quyền riêng tư người dùng
- Chỉ sử dụng cho mục đích hợp pháp

### Troubleshooting

**Lỗi "Navigation timeout"**:
- Kiểm tra kết nối internet
- Tăng timeout trong code
- Thử lại sau vài phút

**Lỗi "Element not found"**:
- Facebook có thể đã thay đổi cấu trúc HTML
- Cập nhật selectors trong code

**Lỗi "Access denied"**:
- Bài viết có thể không public
- Kiểm tra URL bài viết

## 🔒 Bảo mật

- Sử dụng Helmet để bảo vệ headers
- CORS được cấu hình đúng cách
- Input validation cho tất cả API endpoints
- Error handling để tránh lộ thông tin nhạy cảm

## 🚀 Deployment

### Heroku
```bash
# Tạo Procfile
echo "web: node server.js" > Procfile

# Deploy
heroku create
git push heroku main
```

### Docker
```dockerfile
FROM node:16-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 3000
CMD ["npm", "start"]
```

## 📝 License

MIT License - xem file LICENSE để biết thêm chi tiết.

## 🤝 Đóng góp

1. Fork project
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## 📞 Hỗ trợ

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra phần Troubleshooting
2. Tạo issue trên GitHub
3. Mô tả chi tiết lỗi và cách reproduce