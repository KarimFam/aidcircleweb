# AidCircle UI Quick Start Guide

## 🚀 Running the Modernized UI

### Prerequisites
- .NET 8 SDK installed
- Azure AD B2C configured (already set up)
- Database connection string in `appsettings.json`

### Step 1: Build the Solution
```powershell
cd o:\source\repos\aidcircleweb
dotnet build H4H.sln
```

### Step 2: Run the Web Application
```powershell
cd H4H.Presentation.Web\H4H.Presentation.Web
dotnet run
```

### Step 3: Access the Application
- **URL**: `https://localhost:5001` (or the port shown in console)
- **Welcome Page**: Opens automatically for unauthenticated users
- **Dashboard**: Accessible after sign-in at `https://localhost:5001/`

---

## 🎨 What's New in the UI

### 1. Welcome Page (`/welcome`)
- **URL**: Automatic landing for unauthenticated users
- **Features**:
  - Hero section with mission statement
  - Feature cards explaining the platform
  - Call-to-action buttons for sign-in
  - Statistics showcase
  - Responsive footer

### 2. Dashboard (`/` - requires login)
- **Authentication**: Redirects to welcome if not logged in
- **Features**:
  - Personalized greeting
  - Three stat cards (orders, items, volunteers)
  - Quick action buttons
  - Recent activity feed

### 3. Design System
- **File**: `wwwroot/css/aidcircle-design-system.css`
- **Usage**: CSS custom properties available globally
- **Example**:
  ```html
  <div class="ac-card">
      <h3 class="ac-heading-3">Title</h3>
      <p class="ac-text-body">Content</p>
  </div>
  ```

### 4. Reusable Components
All located in `Components/Shared/`:

#### LoadingSpinner
```razor
<LoadingSpinner Message="Loading..." Size="large" />
```

#### Card
```razor
<Card Title="My Card">
    <p>Card content goes here</p>
</Card>
```

#### StatCard
```razor
<StatCard Value="123" Title="Active Orders" Subtitle="This month">
    <IconContent>
        <svg><!-- icon here --></svg>
    </IconContent>
</StatCard>
```

---

## 📱 Mobile-First Design

### Testing on Mobile
1. Open browser DevTools (F12)
2. Click device toolbar icon (Ctrl+Shift+M)
3. Select device (e.g., iPhone 12 Pro)
4. Test navigation and interactions

### Breakpoints
- **Mobile**: < 768px (stacked layouts)
- **Tablet**: 768px - 1024px (2-column grids)
- **Desktop**: > 1024px (full layouts)

---

## 🎤 Microphone Feature (Chat)

### Browser Requirements
- **HTTPS required** (or localhost for development)
- **Permissions**: Browser will prompt for microphone access

### JavaScript API
Located in `wwwroot/js/chat-utils.js`:

```javascript
// Check support
const supported = aidCircleChat.isMicrophoneSupported();

// Start recording
await aidCircleChat.startRecording();

// Stop and get audio
const audioBase64 = await aidCircleChat.stopRecording();
```

### Using in Blazor
```csharp
@inject IJSRuntime JS

private async Task StartRecording()
{
    var success = await JS.InvokeAsync<bool>("aidCircleChat.startRecording");
}

private async Task StopRecording()
{
    var audio = await JS.InvokeAsync<string>("aidCircleChat.stopRecording");
    // Process audio...
}
```

---

## 🔧 Common Tasks

### Adding a New Page
1. Create `.razor` file in `Components/Pages/`
2. Add page directive: `@page "/mypage"`
3. Add authorization: `@attribute [Authorize]`
4. Use design system classes for styling

**Example**:
```razor
@page "/mypage"
@attribute [Authorize]

<PageTitle>My Page</PageTitle>

<div class="ac-container ac-section">
    <h1 class="ac-heading-1">My Page</h1>
    <Card Title="Welcome">
        <p class="ac-text-body">Content here</p>
    </Card>
</div>
```

### Styling with Design System
```html
<!-- Spacing -->
<div class="ac-mt-lg ac-mb-xl">...</div>

<!-- Layout -->
<div class="ac-grid ac-grid-cols-1 ac-grid-cols-md-3">...</div>

<!-- Typography -->
<h2 class="ac-heading-2">Heading</h2>
<p class="ac-text-body">Body text</p>

<!-- Buttons -->
<button class="ac-btn ac-btn-primary ac-btn-lg">Click Me</button>

<!-- Cards -->
<div class="ac-card">
    <div class="ac-card-header">
        <h3 class="ac-card-title">Title</h3>
    </div>
    <div class="ac-card-body">Content</div>
</div>
```

### Adding Images
1. Place images in `wwwroot/images/`
2. Reference in components:
   ```html
   <img src="/images/hero/hero-main.jpg" alt="Description" />
   ```

### Custom CSS for a Page
Create `PageName.razor.css` alongside `PageName.razor`:
```css
/* Automatically scoped to component */
.my-custom-class {
    color: var(--ac-primary);
}
```

---

## 🐛 Troubleshooting

### Welcome Page Doesn't Show
**Issue**: Goes directly to login
**Fix**: Clear browser cache, restart app

### CSS Not Applied
**Check**:
1. `App.razor` includes CSS link
2. File exists at `wwwroot/css/aidcircle-design-system.css`
3. Hard refresh browser (Ctrl+F5)

### Components Not Found
**Error**: "Found markup element with unexpected name 'Card'"
**Fix**: Verify `_Imports.razor` has:
```csharp
@using H4H.Presentation.Web.Components.Shared
```

### Microphone Not Working
**Check**:
1. Using HTTPS (required for getUserMedia)
2. Browser supports microphone (modern Chrome, Edge, Firefox)
3. Permissions granted in browser
4. JavaScript file loaded: `wwwroot/js/chat-utils.js`

### Redirect Loop
**Issue**: Infinite redirect between welcome and home
**Fix**: Check authentication state in browser DevTools > Application > Cookies

---

## 📊 Performance Tips

### Image Optimization
- Use WebP format for modern browsers
- Compress images (TinyPNG, ImageOptim)
- Set explicit width/height to prevent layout shift

### CSS Performance
- Design system CSS loads once, applies globally
- Component-scoped CSS (`*.razor.css`) isolates styles
- Use CSS custom properties instead of inline styles

### JavaScript Performance
- `chat-utils.js` loads once on app start
- Functions are reusable across components
- No heavy libraries (vanilla JS only)

---

## 🎯 Feature Roadmap

### ✅ Phase 1 - Complete
- Responsive design system
- Welcome/landing page
- Modern dashboard
- Reusable components
- Microphone utilities (JS)

### 🚧 Phase 2 - In Progress
- Enhanced chat UI with microphone button
- Session history sidebar
- Voice recording indicator
- Message bubbles redesign

### 📅 Phase 3 - Planned
- Azure Speech integration
- SignalR streaming responses
- Voice-to-text in chat
- Real-time typing indicators

### 🔮 Phase 4 - Future
- Dark mode toggle
- Accessibility enhancements (ARIA)
- Offline mode support
- PWA capabilities

---

## 📞 Need Help?

### Documentation
- **Full Summary**: `UI-MODERNIZATION-SUMMARY.md`
- **Copilot Instructions**: `.github/copilot-instructions.md`
- **Image Assets**: `wwwroot/images/IMAGE-ASSETS-README.md`

### Code References
- **Design System**: `wwwroot/css/aidcircle-design-system.css`
- **Chat Utils**: `wwwroot/js/chat-utils.js`
- **Components**: `Components/Shared/*.razor`

### Testing Checklist
- [ ] Welcome page loads on `https://localhost:5001`
- [ ] Sign-in redirects to dashboard
- [ ] Dashboard shows personalized greeting
- [ ] Stat cards display numbers
- [ ] Mobile responsive (test in DevTools)
- [ ] JavaScript console has no errors
- [ ] CSS loads correctly (no unstyled content)

---

**Last Updated**: November 8, 2025  
**Branch**: v1-ui-update  
**Status**: Phase 1 Complete ✅
