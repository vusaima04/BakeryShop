# BakeryShop - Website bán bánh ngọt

Bài tập lớn học phần **Lập trình web** (ASP.NET Core MVC). BakeryShop là website giới thiệu và bán bánh ngọt kiểu Pháp trực tuyến, gồm phần dành cho **khách hàng** (xem menu, đặt hàng) và phần dành cho **quản trị viên** (quản lý sản phẩm, danh mục, đơn hàng, thống kê).

## 1. Chức năng chính

**Khách vãng lai / khách hàng**

- Xem trang chủ, menu sản phẩm; tìm kiếm theo tên, lọc theo danh mục, sắp xếp theo giá, phân trang
- Xem chi tiết sản phẩm, sản phẩm liên quan, **sản phẩm đã xem gần đây** (lưu bằng Session)
- Đăng ký, đăng nhập, quản lý tài khoản (ASP.NET Core Identity), khóa tài khoản tạm thời khi đăng nhập sai nhiều lần
- Thêm vào giỏ hàng bằng **AJAX** (không tải lại trang), chọn số lượng, đổi số lượng ngay trong giỏ (nút +, − hoặc nhập số), xóa món khỏi giỏ
- Đặt hàng (địa chỉ, số điện thoại, ngày nhận, ghi chú), xem lịch sử và chi tiết đơn hàng
- Chuyển đổi ngôn ngữ **Tiếng Việt / English** (lưu bằng Cookie)

**Quản trị viên (Admin)**

- Quản lý sản phẩm: thêm, sửa, xóa, tải ảnh lên, tìm kiếm, lọc, phân trang
- Quản lý danh mục: thêm, sửa, xóa
- Quản lý đơn hàng: lọc theo trạng thái, xem chi tiết, cập nhật trạng thái (Chờ xác nhận, Đã xác nhận, Đã giao, Đã hủy)
- Trang **Thống kê**: doanh thu, số đơn, top sản phẩm bán chạy, biểu đồ doanh thu 6 tháng
- **RESTful Web API** có Swagger

## 2. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Nền tảng | ASP.NET Core 8 MVC + Razor Pages (Identity UI) |
| Cơ sở dữ liệu | SQL Server (LocalDB), Entity Framework Core 8 (Code First, Migrations) |
| Xác thực, phân quyền | ASP.NET Core Identity, 2 vai trò: `Admin`, `Customer` |
| Giao diện | Bootstrap 5, Bootstrap Icons, jQuery, CSS tùy biến, Chart.js |
| Quốc tế hóa | `IStringLocalizer` + file resource `.resx` (vi, en) |
| API | `[ApiController]`, Swagger (Swashbuckle) |

## 3. Yêu cầu cài đặt

- Windows, **Visual Studio** có workload *ASP.NET and web development* và *.NET 8 SDK*
- **SQL Server LocalDB** (đi kèm Visual Studio). Kiểm tra bằng lệnh `sqllocaldb info`

## 4. Hướng dẫn cài đặt và chạy

### Cách 1: để ứng dụng tự tạo database (khuyên dùng)

1. Giải nén mã nguồn, mở file `BakeryShop.slnx` bằng Visual Studio.
2. Kiểm tra chuỗi kết nối trong `BakeryShop/appsettings.json` (mặc định dùng LocalDB, không cần sửa):

   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BakeryShopDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   ```

3. Nhấn **F5** (profile `http`). Lần chạy đầu ứng dụng tự động:
   - tạo database `BakeryShopDb` và các bảng (`Database.MigrateAsync()`),
   - tạo 2 vai trò `Admin`, `Customer`,
   - tạo tài khoản quản trị mặc định,
   - thêm dữ liệu mẫu: 4 danh mục (Bánh Sinh Nhật, Bánh Nhỏ, Bánh Nướng, Combo) và 23 sản phẩm.
4. Trình duyệt mở tại `http://localhost:5079`.

### Cách 2: chạy script SQL

1. Mở SQL Server Management Studio (hoặc *SQL Server Object Explorer* trong Visual Studio), kết nối `(localdb)\MSSQLLocalDB`.
2. Tạo database rỗng tên `BakeryShopDb`, mở file `Database/BakeryShop.sql`, chọn database đó và chạy.
3. Chạy ứng dụng như ở Cách 1. Dữ liệu mẫu và tài khoản admin vẫn được thêm tự động khi ứng dụng khởi động lần đầu.

Tạo lại script từ migration (khi sửa model), trong **Package Manager Console**:

```powershell
Script-Migration -Idempotent -Output Database\BakeryShop.sql
```

### Cách 3: dùng dòng lệnh

```powershell
cd BakeryShop
dotnet ef database update
dotnet run
```

## 5. Tài khoản demo

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Quản trị viên | `admin@bakeryshop.com` | `Admin@123456` |
| Khách hàng | tự đăng ký tại `/Identity/Account/Register` | tùy chọn (tối thiểu theo quy tắc Identity) |

Mật khẩu admin có thể đổi trước lần chạy đầu tiên bằng **User Secrets** (chuột phải project, *Manage User Secrets*):

```json
{ "AdminSeed": { "Password": "MatKhauCuaBan@123" } }
```

Nếu không cấu hình, hệ thống dùng mật khẩu mặc định ở bảng trên. Tài khoản admin chỉ được tạo khi chưa tồn tại, nên muốn đổi mật khẩu sau này hãy dùng chức năng *Quản lý tài khoản* hoặc xóa database và chạy lại.

## 6. Các trang chính

| Đường dẫn | Mô tả | Quyền |
|---|---|---|
| `/` | Trang chủ | Mọi người |
| `/Home/Menu` | Menu: tìm kiếm, lọc, sắp xếp, phân trang | Mọi người |
| `/Home/Detail/{id}` | Chi tiết sản phẩm | Mọi người |
| `/Cart` | Giỏ hàng | Đăng nhập |
| `/Order/Checkout`, `/Order/History` | Đặt hàng, lịch sử đơn | Đăng nhập |
| `/Products` | Quản lý sản phẩm | Admin |
| `/Categories` | Quản lý danh mục | Admin |
| `/AdminOrders` | Quản lý đơn hàng | Admin |
| `/Dashboard` | Thống kê | Admin |
| `/swagger` | Tài liệu và thử API (chỉ môi trường Development) | Mọi người |

## 7. RESTful Web API

Địa chỉ gốc: `http://localhost:5079`. Xem và thử trực tiếp tại `/swagger`.

| Phương thức | Đường dẫn | Mô tả | Quyền | Mã trả về |
|---|---|---|---|---|
| GET | `/api/products?search=&categoryId=&page=1&pageSize=10` | Danh sách sản phẩm, có tìm kiếm, lọc, phân trang | Mọi người | 200 |
| GET | `/api/products/{id}` | Chi tiết một sản phẩm | Mọi người | 200, 404 |
| POST | `/api/products` | Thêm sản phẩm | Admin | 201, 400, 401, 403 |
| PUT | `/api/products/{id}` | Sửa sản phẩm | Admin | 204, 400, 401, 403, 404 |
| DELETE | `/api/products/{id}` | Xóa sản phẩm | Admin | 204, 401, 403, 404 |
| GET | `/api/categories` | Danh sách danh mục kèm số sản phẩm | Mọi người | 200 |

API xác thực bằng cookie đăng nhập của website. Đăng nhập bằng tài khoản Admin trên trình duyệt rồi gọi API ở cùng trình duyệt (hoặc dùng Swagger). Các yêu cầu không hợp lệ trả về `401` (chưa đăng nhập) hoặc `403` (không đủ quyền) thay vì chuyển hướng.

## 8. Cấu trúc thư mục

```
BakeryShop.slnx
Database/BakeryShop.sql         Script tạo cơ sở dữ liệu
BakeryShop/
  Controllers/                  MVC controllers (Home, Cart, Order, Products, Categories, AdminOrders, Dashboard)
  Controllers/API/              REST API (ProductsApi, CategoriesApi)
  Models/                       Entity + validation (Data Annotations)
  Data/                         ApplicationDbContext, SeedData
  Migrations/                   EF Core migrations
  Areas/Identity/               Giao diện đăng nhập, đăng ký (Identity UI)
  Views/                        Razor views, layout, partial
  SharedResource.en.resx        Bản dịch tiếng Anh
  wwwroot/                      CSS, JS, hình ảnh
```

## 9. Một số lưu ý

- **Ảnh sản phẩm**: ảnh tải lên từ trang quản lý được lưu trong `wwwroot/Images/products`. Nút *Dọn ảnh thừa* xóa các file ảnh không còn sản phẩm nào dùng. Có thể chép sẵn ảnh đặt tên theo tên sản phẩm không dấu (ví dụ `pink-guava.png`) vào thư mục này để seed tự gắn cho sản phẩm.
- **Chuyển ngôn ngữ**: nút `VI | EN` ở góc trên bên phải, lựa chọn được lưu trong cookie `.AspNetCore.Culture`.
- **Gửi email**: dự án dùng `FakeEmailSender` (không gửi thật), nên các luồng xác nhận email không gửi thư.
- Không xóa được sản phẩm đã nằm trong đơn hàng, nhằm giữ nguyên lịch sử đơn.
