# Facebook Comment Analyzer

Ứng dụng phân tích comment từ bài viết Facebook, cho phép tìm kiếm, sắp xếp và kiểm tra xem người comment có share bài viết hay không.

## Tính năng

- 📊 Lấy thông tin bài viết Facebook
- 💬 Phân tích tất cả comment của bài viết
- 🔍 Tìm kiếm comment theo nội dung hoặc tên người dùng
- 📅 Sắp xếp comment theo thời gian, số like, hoặc trạng thái share
- ✅ Kiểm tra xem người comment có share bài viết hay không
- 🎨 Giao diện đẹp và responsive

## Cấu trúc Project

```
facebook-comment-analyzer/
├── backend/                          # .NET Core MVC API
│   └── FacebookCommentAnalyzer.API/
│       ├── Controllers/              # API Controllers
│       ├── Models/                   # Data Models
│       ├── Services/                 # Business Logic
│       └── Program.cs                # Application Entry Point
└── frontend/                         # Vue.js Frontend
    └── facebook-comment-frontend/
        ├── src/
        │   ├── components/           # Vue Components
        │   ├── views/                # Page Views
        │   ├── services/             # API Services
        │   └── router/               # Vue Router
        └── package.json
```

## Yêu cầu hệ thống

- .NET 8.0 SDK
- Node.js 16+ và npm
- Facebook Access Token với quyền truy cập vào bài viết

## Cài đặt và chạy

### Backend (.NET Core)

1. Cài đặt .NET 8.0 SDK
2. Di chuyển vào thư mục backend:
   ```bash
   cd facebook-comment-analyzer/backend/FacebookCommentAnalyzer.API
   ```

3. Khôi phục dependencies:
   ```bash
   dotnet restore
   ```

4. Chạy ứng dụng:
   ```bash
   dotnet run
   ```

Backend sẽ chạy tại: `http://localhost:5000`

### Frontend (Vue.js)

1. Di chuyển vào thư mục frontend:
   ```bash
   cd facebook-comment-analyzer/frontend/facebook-comment-frontend
   ```

2. Cài đặt dependencies:
   ```bash
   npm install
   ```

3. Chạy ứng dụng development:
   ```bash
   npm run dev
   ```

Frontend sẽ chạy tại: `http://localhost:5173`

## Sử dụng

1. Mở trình duyệt và truy cập `http://localhost:5173`
2. Nhập Facebook Post ID và Access Token
3. Click "Get Post Info" để xem thông tin bài viết
4. Click "Analyze Comments" để phân tích tất cả comment
5. Sử dụng thanh tìm kiếm và bộ lọc để tìm comment mong muốn

## API Endpoints

### Backend API

- `GET /api/facebook/post/{postId}` - Lấy thông tin bài viết
- `GET /api/facebook/post/{postId}/comments` - Lấy tất cả comment
- `GET /api/facebook/post/{postId}/analyze` - Phân tích comment và kiểm tra share
- `GET /api/facebook/user/{userId}/check-share` - Kiểm tra user có share bài không

## Lưu ý quan trọng

⚠️ **Facebook API Limitations:**
- Cần Facebook Access Token với quyền truy cập phù hợp
- Một số bài viết có thể không thể truy cập do cài đặt quyền riêng tư
- Rate limiting có thể áp dụng cho API calls

⚠️ **Bảo mật:**
- Không chia sẻ Access Token với người khác
- Token có thể hết hạn, cần refresh khi cần thiết
- Chỉ sử dụng cho mục đích phân tích hợp pháp

## Công nghệ sử dụng

### Backend
- .NET Core 8.0
- ASP.NET Core MVC
- Newtonsoft.Json
- HttpClient

### Frontend
- Vue.js 3
- Vue Router
- Axios
- Vite

## Đóng góp

1. Fork project
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## License

MIT License - xem file LICENSE để biết thêm chi tiết.