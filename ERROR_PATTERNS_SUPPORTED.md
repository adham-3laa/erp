# اختبار تعريب رسائل الأخطاء - أمثلة

## الأخطاء المختبرة:

### ✅ 1. نقص المخزون
**الأصلي:**
```
Request failed: 400 Bad Request
Insufficient stock for product 'شاشه'
```
**المعرب:**
```
الكمية المتوفرة غير كافية للمنتج 'شاشه'
```

---

### ✅ 2. منتج غير موجود - Pattern 1
**الأصلي:**
```
Product with Name شاشششه not found.
```
**المعرب:**
```
المنتج 'شاشششه' غير موجود
```

---

### ✅ 3. منتج غير موجود - Pattern 2
**الأصلي:**
```
Product 'لابتوب' not found
```
**المعرب:**
```
المنتج 'لابتوب' غير موجود
```

---

### ✅ 4. منتج غير موجود - Pattern 3
**الأصلي:**
```
Product ماوس not found
```
**المعرب:**
```
المنتج 'ماوس' غير موجود
```

---

### ✅ 5. منتج غير متوفر
**الأصلي:**
```
Product 'كيبورد' is out of stock
```
**المعرب:**
```
المنتج 'كيبورد' غير متوفر في المخزون
```

---

### ✅ 6. مندوب غير موجود - NEW!
**الأصلي:**
```
Sales Representative not found
```
**المعرب:**
```
المندوب غير موجود
```

---

### ✅ 7. عميل غير موجود - NEW!
**الأصلي:**
```
Customer not found
```
**المعرب:**
```
العميل غير موجود
```

---

### ✅ 8. مورد غير موجود - NEW!
**الأصلي:**
```
Supplier not found
```
**المعرب:**
```
المورد غير موجود
```

---

## الـ Patterns المدعومة الآن:

### لأخطاء "Product Not Found":
1. `Product with Name X not found` ← **جديد!**
2. `Product with Name 'X' not found` ← **جديد!**
3. `Product 'X' not found`
4. `Product X not found` (بدون علامات تنصيص)

### لأخطاء "User/Sales Rep/Customer Not Found": ← **جديد!**
1. `Sales Representative not found` → `المندوب غير موجود`
2. `Sales Representative with name X not found` → `المندوب 'X' غير موجود`
3. `Customer not found` → `العميل غير موجود`
4. `Customer with name X not found` → `العميل 'X' غير موجود`
5. `User not found` → `المستخدم غير موجود`
6. `Supplier not found` → `المورد غير موجود`

### لأخطاء "Insufficient Stock":
1. `Insufficient stock for product 'X'`
2. `Insufficient stock`

### لأخطاء "Out of Stock":
1. `Product 'X' is out of stock`
2. `Product X out of stock`

---

## كيفية الاستخدام:

```csharp
using erp.Helpers;

// في أي مكان عند معالجة الأخطاء:
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

---

**آخر تحديث**: 2026-01-24 16:35
**الحالات المدعومة**: 14+ نمط مختلف (منتجات، مستخدمين، عملاء، مندوبين، موردين)
