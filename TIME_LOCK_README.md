# 🔐 WPF Time-Lock Security System

## Overview

This is a **permanent time-based security overlay** system for WPF MVVM applications. Starting from a specified date (February 1st, 2026), the application will display a full-screen lock overlay that blocks all access until the correct password is entered. Once unlocked, the application remains unlocked **forever**, even across restarts and system date changes.

![Lock Overlay Mockup](./lock_overlay_preview.png)

---

## 🎯 Features

### Core Functionality
- ✅ **Date-Based Activation**: Locks automatically on or after February 1st, 2026
- ✅ **Full-Screen Blocking**: Completely blocks all UI interaction
- ✅ **Password Protection**: Single unlock password (119119)
- ✅ **Permanent Unlock**: Once unlocked, never locks again
- ✅ **Unlimited Attempts**: No lockout, delays, or cooldowns

### Security Features
- 🔐 **AES-256 Encryption**: Unlock state stored in encrypted format
- 🔐 **Machine Binding**: Lock file tied to specific hardware
- 🔐 **SHA256 Hashing**: Validation hash prevents tampering
- 🔐 **Time Rollback Protection**: Changing date backwards doesn't re-lock
- 🔐 **Tamper Detection**: Invalid files treated as locked state

### User Experience
- 🎨 **Premium Arabic UI**: Modern, clean, professional design
- 🎨 **Smooth Animations**: Polished visual transitions
- 🎨 **Clear Feedback**: Informative error messages
- 🎨 **Keyboard Support**: Enter key unlocks
- 🎨 **Auto-Focus**: Password field focused on load

---

## 📊 System Architecture

### Components

```
┌─────────────────────────────────────────┐
│         App Startup (App.xaml.cs)      │
│  - Initialize LockService               │
│  - Check lock status                    │
│  - Show overlay or login                │
└─────────────────────────────────────────┘
                    ↓
         ┌──────────┴──────────┐
         │                     │
    [LOCKED]              [UNLOCKED]
         │                     │
         ↓                     ↓
┌─────────────────┐   ┌──────────────┐
│  Lock Overlay   │   │ Login Window │
│  - Password UI  │   │ (Normal flow)│
│  - Validation   │   └──────────────┘
└─────────────────┘
         │
    [Correct Password]
         │
         ↓
┌─────────────────────────────┐
│   LockService               │
│   - Encrypt unlock state    │
│   - Save to AppData         │
│   - Mark as permanently     │
│     unlocked                │
└─────────────────────────────┘
```

### File Structure

```
erp/
├── Services/
│   └── LockService.cs              # Core lock logic
├── ViewModels/
│   └── SystemLockOverlayViewModel.cs  # Overlay ViewModel
├── Views/
│   ├── SystemLockOverlay.xaml      # Overlay UI
│   └── SystemLockOverlay.xaml.cs   # Overlay code-behind
└── App.xaml.cs                      # Modified for lock integration

Documentation/
├── TIME_LOCK_DOCUMENTATION.md      # Full technical docs
├── TESTING_GUIDE.md                # Testing instructions
└── IMPLEMENTATION_SUMMARY.md       # This file
```

---

## 🚀 Quick Start

### Installation
The system is already integrated into your application. No additional setup required!

### Usage

#### Before February 1, 2026
Application works normally - no changes to user experience.

#### On or After February 1, 2026 (First Launch)
1. Application shows lock overlay
2. User enters password: `119119`
3. Application unlocks permanently
4. Login window appears

#### After Unlocking
Application works normally forever - lock never appears again.

---

## 🔑 Configuration

### Lock Date
```csharp
// In LockService.cs
private static readonly DateTime LOCK_DATE = new DateTime(2026, 2, 1);
```

### Unlock Password
```csharp
// In LockService.cs
private const string UNLOCK_PASSWORD = "119119";
```

⚠️ **Note**: Changes require recompilation.

---

## 🧪 Testing

### Quick Test - Trigger Lock Now

1. **Change System Date**:
   ```
   Settings → Time & Language → Date & Time
   Turn off "Set time automatically"
   Set date to: February 1, 2026
   ```

2. **Run Application**:
   - Lock overlay should appear
   - Enter password: `119119`
   - Overlay should close and show login

3. **Verify Persistence**:
   - Close and restart application
   - Lock should NOT appear
   - Change date back to January 2026
   - Restart - lock still should NOT appear

### Complete Test Suite
See `TESTING_GUIDE.md` for detailed test scenarios.

---

## 📁 Data Storage

### Lock File Location
```
%AppData%\ERPSystem\.syslock
```

### File Contents (Encrypted)
```
UNLOCKED|{timestamp}|{machineId}|{validationHash}
```

### Reset Lock State (Testing)
```powershell
# Close application first
Remove-Item "$env:APPDATA\ERPSystem\.syslock"
# Set date to Feb 1, 2026
# Restart application → Lock appears again
```

---

## 🛡️ Security Details

### Encryption
- **Algorithm**: AES-256 CBC
- **Key Source**: SHA256 hash of machine name + OS version
- **IV**: Random, prepended to ciphertext

### Validation
- **Unlock Data Hash**: SHA256 validation
- **Machine ID**: Hashed machine + user name
- **Tamper Detection**: Invalid data = locked state

### Anti-Bypass Measures
1. ✅ Encrypted storage prevents manual editing
2. ✅ Machine binding prevents file copying
3. ✅ Hash validation prevents tampering
4. ✅ Time rollback resistant (unlock is permanent)
5. ✅ Window cannot be closed (Alt+F4 blocked)
6. ✅ No task manager end-task bypass

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| `TIME_LOCK_DOCUMENTATION.md` | Complete technical documentation |
| `TESTING_GUIDE.md` | Step-by-step testing instructions |
| `IMPLEMENTATION_SUMMARY.md` | Executive summary and overview |
| This README | Quick reference guide |

---

## 🔍 Troubleshooting

### Problem: Lock doesn't appear on Feb 1, 2026
**Solution**: 
- Verify system date is correct
- Check `LockService` is initialized in `App.xaml.cs`
- Review application logs

### Problem: Correct password doesn't unlock
**Solution**:
- Ensure password is exactly `119119` (no spaces)
- Check application logs for exceptions
- Verify `UnlockCommand` binding in XAML

### Problem: Lock appears again after unlocking
**Solution**:
- Check if `.syslock` file exists in `%AppData%\ERPSystem\`
- Verify file permissions (read/write)
- Check for encryption errors in logs

### Problem: Cannot type in password field
**Solution**:
- Verify `PasswordBox` is enabled
- Check if focus is set correctly
- Try clicking on the password field

---

## ⚠️ Important Production Notes

### Before Deployment
1. ✅ Test all scenarios thoroughly
2. ✅ Verify lock date is correct
3. ✅ Confirm unlock password is documented
4. ✅ Test on clean machine (no .syslock file)
5. ✅ Backup unlock password securely

### User Communication
- Inform users before Feb 1, 2026
- Provide unlock password to authorized personnel
- Document unlock process for support team
- Plan for unlock assistance on activation date

### Password Security
- Store unlock password in secure location
- Limit distribution to authorized personnel only
- Consider changing password before production (requires recompile)
- Have backup plan if password is lost

---

## 📊 Requirements Compliance

| Requirement | Implementation | Status |
|-------------|----------------|--------|
| Lock from Feb 1, 2026 | Date check in `LockService.IsSystemLocked()` | ✅ |
| Full-screen blocking | `SystemLockOverlay` with `Topmost=True` | ✅ |
| Block all interaction | Transparent overlay + no close button | ✅ |
| Password: 119119 | `LockService.UNLOCK_PASSWORD` | ✅ |
| Permanent unlock | Encrypted `.syslock` file | ✅ |
| Survives restarts | Persistent file storage | ✅ |
| Time rollback protection | Unlock check doesn't depend on date | ✅ |
| Unlimited attempts | No attempt counter or lockout | ✅ |
| No delays | Instant retry allowed | ✅ |
| Encrypted storage | AES-256 encryption | ✅ |
| Anti-bypass | Multiple security layers | ✅ |
| MVVM architecture | Separate Service/ViewModel/View | ✅ |
| Arabic UI | RTL support, Arabic text | ✅ |
| Premium design | Modern, polished interface | ✅ |

---

## 🎨 UI Preview

The lock overlay features a premium, modern design:

- **Full-screen dark overlay** with blur effect
- **Centered white card** with drop shadow
- **Red lock icon** symbolizing security
- **Arabic title and message**
- **Clean password input field**
- **Blue unlock button**
- **Error message display area**
- **Processing indicator**

---

## 🔄 Workflow

### Locked State (Feb 1+ First Time)
```
App Start → Lock Check → Locked → Show Overlay
→ User Enters Password → Validate
→ If Wrong: Show Error, Allow Retry
→ If Correct: Save Unlock State → Close Overlay → Show Login
```

### Unlocked State (After First Unlock)
```
App Start → Lock Check → Already Unlocked → Show Login Directly
```

---

## 📞 Support Information

**Unlock Password**: `119119`  
**Lock Activation**: February 1, 2026, 00:00  
**Lock File**: `%AppData%\ERPSystem\.syslock`  
**Current Status**: 7 days until activation (from Jan 25, 2026)

---

## ✅ Checklist for Production

- [ ] Build application successfully
- [ ] Test lock activation (change date to Feb 1, 2026)
- [ ] Test wrong password attempts
- [ ] Test correct password unlock (119119)
- [ ] Test persistence across restarts
- [ ] Test time rollback protection
- [ ] Document unlock password for support team
- [ ] Communicate to users before Feb 1, 2026
- [ ] Create support documentation
- [ ] Plan for unlock assistance on activation date

---

## 🎉 Success Criteria

Your time-lock system is successfully implemented when:

✅ Application runs normally before Feb 1, 2026  
✅ Lock overlay appears on or after Feb 1, 2026  
✅ Wrong passwords show error and allow retry  
✅ Password `119119` unlocks the system  
✅ After unlock, app never locks again  
✅ Changing system date doesn't re-lock  
✅ Unlock state persists across restarts  

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Jan 25, 2026 | Initial implementation |

---

## 📄 License

Part of the WPF ERP application - Internal use only.

---

## 🤝 Contributing

This is a security-critical component. Any modifications should be:
1. Thoroughly tested
2. Reviewed by security team
3. Documented completely
4. Approved before deployment

---

**🔐 Your application is now secured with a permanent time-lock system!**

For questions or issues, refer to the comprehensive documentation in:
- `TIME_LOCK_DOCUMENTATION.md` - Full technical reference
- `TESTING_GUIDE.md` - Testing procedures
- `IMPLEMENTATION_SUMMARY.md` - Executive summary
