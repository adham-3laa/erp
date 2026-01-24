# تعريب رسائل الأخطاء في التطبيق

## التحديثات المنفذة

### 1. إنشاء ErrorMessageLocalizer Helper
تم إنشاء ملف `Helpers/ErrorMessageLocalizer.cs` لتعريب جميع رسائل الأخطاء القادمة من API تلقائياً.

#### المميزات:
- **تعريب تلقائي**: جميع رسائل الأخطاء الإنجليزية تُترجم إلى العربية
- **معالجة خاصة للمخزون**: رسائل مثل `Insufficient stock for product 'شاشه'` تُترجم إلى `الكمية المتوفرة غير كافية للمنتج 'شاشه'`
- **قاموس شامل**: يحتوي على ترجمات لأكثر من 30 رسالة خطأ شائعة
- **تنظيف الرسائل**: إزالة تفاصيل تقنية غير ضرورية (مثل Status Code)

#### الأخطاء المعالجة:

##### أخطاء المخزون
- `Insufficient stock for product 'X'` → `الكمية المتوفرة غير كافية للمنتج 'X'`
- `Out of stock` → `المنتج غير متوفر في المخزون`
- `Product not found` → `المنتج غير موجود`

##### أخطاء التحقق
- `Invalid input` → `البيانات المدخلة غير صحيحة`
- `Required field` → `هذا الحقل مطلوب`
- `Invalid format` → `الصيغة غير صحيحة`

##### أخطاء المستخدم والصلاحيات
- `User not found` → `المستخدم غير موجود`
- `Unauthorized` → `غير مصرح لك بهذا الإجراء`
- `Access denied` → `تم رفض الوصول`

##### أخطاء الشبكة
- `Network error` → `خطأ في الاتصال بالشبكة`
- `Connection failed` → `فشل الاتصال`
- `Timeout` → `انتهت مهلة الاتصال`
- `Server error` → `خطأ في الخادم`

### 2. تحديث CreateOrderPage
تم تحديث `Views/Orders/CreateOrderPage.xaml.cs` لاستخدام ErrorMessageLocalizer:

```csharp
catch (Exception ex)
{
    SetLoading(false);
    
    // تعريب رسالة الخطأ
    var localizedError = ErrorMessageLocalizer.GetLocalizedErrorFromException(ex);
    
    SetStatus($"فشل في إنشاء الطلب", StatusType.Error);

    MessageBox.Show(
        $"حدث خطأ أثناء إنشاء الطلب:\n{localizedError}",
        "خطأ",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
}
```

## كيفية الاستخدام

### استخدام بسيط
```csharp
using erp.Helpers;

try
{
    // Your API call
}
catch (Exception ex)
{
    var arabicError = ErrorMessageLocalizer.GetLocalizedErrorFromException(ex);
    MessageBox.Show(arabicError, "خطأ");
}
```

### تعريب رسالة خطأ محددة
```csharp
string apiError = "Insufficient stock for product 'شاشه'";
string arabicError = ErrorMessageLocalizer.LocalizeError(apiError);
// النتيجة: "الكمية المتوفرة غير كافية للمنتج 'شاشه'"
```

## الأماكن التي يجب تحديثها (اختياري)

يمكنك استخدام ErrorMessageLocalizer في الأماكن التالية لتعريب جميع الأخطاء:

1. **ApprovedOrdersPage.xaml.cs** (سطر 57)
2. **SalesRepOrdersPage.xaml.cs** (سطور 38, 56)
3. **StockIn ProductsPage.xaml.cs** (سطر 459)
4. **InventoryPage.xaml.cs** (سطور 120, 255, 339)
5. **AddNewItem.xaml.cs** (سطر 242)

مثال التحديث:
```csharp
// قبل:
MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ");

// بعد:
var localizedError = ErrorMessageLocalizer.GetLocalizedErrorFromException(ex);
MessageBox.Show($"حدث خطأ: {localizedError}", "خطأ");
```

## ملاحظات مهمة

1. **التكامل مع ErrorHandlingService**: الـ Helper الجديد يكمل ErrorHandlingService الموجود، ولا يحل محله
2. **Logging**: جميع الأخطاء الأصلية يتم حفظها في الـ logs للمطورين
3. **قابلية التوسع**: يمكن إضافة ترجمات جديدة بسهولة في قاموس `ErrorTranslations`

## أمثلة واقعية

### قبل التحديث:
```
حدث خطأ أثناء إنشاء الطلب:
Request failed: 400 Bad Request
Insufficient stock for product 'شاشه'
```

### بعد التحديث:
```
حدث خطأ أثناء إنشاء الطلب:
الكمية المتوفرة غير كافية للمنتج 'شاشه'
```

---

**تاريخ التحديث**: 2026-01-24
**الملفات المضافة**: 
- `erp/Helpers/ErrorMessageLocalizer.cs`

**الملفات المعدلة**:
- `erp/Views/Orders/CreateOrderPage.xaml.cs`
