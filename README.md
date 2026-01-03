## Product Event Store Example

ASP.NET Core MVC + EventStore + PostgreSQL örneği. Ürün CRUD istekleri doğrudan veritabanına değil, EventStore’a yazılır. Ayrı bir worker servisi bu event’leri dinler ve PostgreSQL’e işler.

### Mimari
- **Product.Application (MVC)**: Kullanıcıdan gelen Create/Update/Delete isteklerini `product-stream`e event olarak basar, ekrana liste/detay gösterimi için PostgreSQL’den okur.
- **Shared**: Event tanımları (`ProductCreatedEvent`, `ProductUpdatedEvent`, `ProductDeletedEvent`) ve EventStore servis sözleşmesi.
- **Product.Event.Handler.Service (Worker)**: EventStore’a abone olur, event’i çözümler ve `Handlers/ProductEventHandler` ile PostgreSQL’de `Products` tablosuna insert/update/delete yapar.

### Teknolojiler
- .NET 9, ASP.NET Core MVC
- EventStore (client: Shared.Services.EventStoreService)
- PostgreSQL (EF Core + Npgsql, DbContextFactory)
- AutoMapper (UI modelleri ↔ entity)

### Kurulum
1) **PostgreSQL bağlantısı**  
   `appsettings.json` ve `appsettings.Development.json` içinde `DefaultConnection` tanımlı:  
   `Host=localhost;Port=5432;Database=TestEventStoreProductDb;Username=ahmet;Password=Ahmet.123`

2) **Bağımlılıkları yükle ve derle**  
   ```bash
   dotnet restore
   dotnet build ProductEventStoreExample.sln
   ```

3) **Migration / DB oluşturma (uygulama tarafı)**  
   ```bash
   dotnet ef database update --project Product.Application --startup-project Product.Application
   ```
   Bu adım `Products` tablosunu ve `IsActive` sütununu ekler.

4) **Servisleri çalıştırma**  
   - MVC uygulaması:  
     ```bash
     dotnet run --project Product.Application
     ```  
     Tarayıcıda `/Product` üzerinden CRUD formları.
   - Worker (event consumer):  
     ```bash
     dotnet run --project Product.Event.Handler.Service
     ```  
     Worker açıkken EventStore’dan gelen event’ler PostgreSQL’e yazılır.

### Event akışı
- **Create**: MVC `ProductController` `ProductCreatedEvent` yayımlar. Worker event’i alır, isim çakışması yoksa `Products` tablosuna ekler.
- **Update**: `ProductUpdatedEvent` yayımlanır; worker ilgili kaydı günceller.
- **Delete**: `ProductDeletedEvent` yayımlanır; worker kaydı siler.

### Önemli Notlar
- Identity alanı (Id) PostgreSQL tarafından üretilir; event’teki Id alanı şu an DB yazımında kullanılmıyor.
- Worker’ın çalışmaması durumunda event’ler DB’ye yansımaz; geliştirme sırasında her iki projeyi de çalıştırın.
- Event türü eşleştirme `Shared` assembly’deki tip isimleriyle yapılır; yeni event eklerken class adını stream’deki `EventType` ile aynı tutun.

### Hızlı doğrulama
1) Worker ve MVC’yi başlatın.  
2) `/Product` üzerinden yeni ürün oluşturun.  
3) PostgreSQL’de kontrol:  
   ```bash
   psql -h localhost -U ahmet -d TestEventStoreProductDb -c "select * from \"Products\" order by \"Id\" desc limit 5;"
   ```
   Yeni kaydı görüyorsanız akış çalışıyor demektir.

