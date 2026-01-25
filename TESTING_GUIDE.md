# 🔐 Quick Testing Guide - Time-Lock System

## Current Status
- **Today's Date**: January 25, 2026
- **Lock Date**: February 1, 2026  
- **Days Until Lock**: 7 days
- **Unlock Password**: `119119`

## Quick Test Instructions

### ✅ Test 1: Normal Operation (Before Lock Date)
**Current behavior - lock should NOT appear**

1. Run the application now (date is Jan 25, 2026)
2. ✅ Expected: Application goes directly to login screen
3. ✅ Expected: No lock overlay appears

---

### 🔒 Test 2: Lock Activation
**Trigger the lock by advancing the date**

1. **Change Windows Date**:
   - Press `Win + I` to open Settings
   - Go to `Time & Language` → `Date & Time`
   - Turn OFF "Set time automatically"
   - Click "Change" and set date to: **February 1, 2026**
   - Click "Change" to save

2. **Start Application**:
   - Run the ERP application
   - ✅ Expected: Full-screen lock overlay appears
   - ✅ Expected: Cannot access anything except password field

3. **Test Wrong Password**:
   - Type: `000000` (or any wrong password)
   - Click "فتح النظام" or press Enter
   - ✅ Expected: Error message in Arabic appears
   - ✅ Expected: Can try again immediately
   - Try 5+ different wrong passwords
   - ✅ Expected: No lockout, can keep trying

4. **Test Correct Password**:
   - Type: `119119`
   - Click "فتح النظام" or press Enter
   - ✅ Expected: Lock overlay closes immediately
   - ✅ Expected: Login window appears

---

### 🔓 Test 3: Permanent Unlock
**Verify lock never appears again after unlock**

1. **After successful unlock from Test 2**:
   - Close the entire application
   - Restart the application
   - ✅ Expected: Goes directly to login (no lock overlay)

2. **Test with Date Change**:
   - Keep date at Feb 1, 2026 (or change to Feb 15, 2026, March 2026, etc.)
   - Restart application multiple times
   - ✅ Expected: Never shows lock overlay again

---

### ⏪ Test 4: Time Rollback Protection
**Verify changing date backwards doesn't re-lock**

1. **Change Date Backwards**:
   - Application still unlocked from Test 3
   - Change Windows date back to January 25, 2026
   - Restart application
   - ✅ Expected: Still no lock overlay (remains unlocked)

2. **Change Date Forward Again**:
   - Change date to Feb 1, 2027 (1 year later)
   - Restart application  
   - ✅ Expected: Still no lock overlay

---

### 🗑️ Test 5: Unlock State Persistence
**Check the unlock state file**

1. **Find Lock File**:
   - Open File Explorer
   - Navigate to: `%AppData%\ERPSystem\`
   - ✅ Expected: File named `.syslock` exists (after unlocking)

2. **Test File Deletion**:
   - Close application
   - Delete the `.syslock` file
   - Change Windows date to Feb 1, 2026 (if not already)
   - Start application
   - ✅ Expected: Lock overlay appears again (need to re-unlock)

3. **Re-unlock**:
   - Enter password `119119` again
   - ✅ Expected: New `.syslock` file is created

---

### 🛠️ Test 6: UI/UX Validation
**Check the overlay design and behavior**

1. **Visual Check**:
   - Trigger lock overlay (date = Feb 1, 2026, no .syslock file)
   - ✅ Check: Full screen dark overlay
   - ✅ Check: Centered white card with lock icon
   - ✅ Check: Arabic text displays correctly
   - ✅ Check: Password field is focused automatically

2. **Interaction Check**:
   - Try to click outside the card
   - ✅ Expected: Cannot interact with anything behind overlay
   - Try pressing `Alt+F4`
   - ✅ Expected: Window does not close
   - Try pressing `Esc`
   - ✅ Expected: Window does not close

3. **Keyboard Check**:
   - Type password in the field
   - Press `Enter` key
   - ✅ Expected: Triggers unlock attempt (same as clicking button)

---

## 🎯 Expected Results Summary

| Test Scenario | Expected Behavior |
|---------------|-------------------|
| Before Feb 1, 2026 | No lock, go to login |
| On/After Feb 1, 2026 (first time) | Lock overlay appears |
| Wrong password | Error message, can retry |
| Correct password (119119) | Unlock permanently |
| After unlock + restart | Never locks again |
| After unlock + date rollback | Still unlocked |
| Delete .syslock file | Lock appears again on next start |

---

## 🔑 Quick Reference

**Unlock Password**: `119119`

**Lock File Location**: `%AppData%\ERPSystem\.syslock`

**Lock Activation Date**: February 1, 2026

**Current Date**: January 25, 2026 (7 days before lock)

---

## 🚨 Reset Lock State (For Testing)

To reset and test again:

1. Close application
2. Navigate to: `%AppData%\ERPSystem\`
3. Delete `.syslock` file
4. Set Windows date to Feb 1, 2026 or later
5. Start application → Lock overlay should appear

---

## ⚠️ Important Notes

- The lock is based on **local system date**, not internet time
- Once unlocked, the state is **permanent** (survives restarts, date changes)
- Deleting the `.syslock` file re-enables the lock
- The overlay **cannot be bypassed** once it appears
- **Unlimited password attempts** are allowed

---

**Quick Command to Open AppData Folder**:
```
Win + R → Type: %AppData%\ERPSystem → Press Enter
```
