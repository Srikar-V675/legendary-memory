# BidSphere Dashboard - Angular 19

## 🚀 Quick Start

### Step 1: Install Dependencies

```bash
cd frontend
npm install
```

### Step 2: Start Development Server

```bash
npm start
```

Dashboard will be available at: `http://localhost:4200`

### Step 3: Ensure API is Running

```bash
# In another terminal
cd BidSphere
dotnet run
```

API should be running on: `http://localhost:8080`

---

## 📁 Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── app.component.ts          # Main component
│   │   ├── app.component.html        # Dashboard template
│   │   ├── app.component.css         # Dashboard styles
│   │   └── dashboard.service.ts      # API service
│   ├── index.html                    # HTML entry point
│   ├── main.ts                       # Angular bootstrap
│   └── styles.css                    # Global styles
├── angular.json                      # Angular configuration
├── package.json                      # Dependencies
├── tsconfig.json                     # TypeScript config
└── tsconfig.app.json                 # App TypeScript config
```

---

## 🎯 Features

- ✅ **Angular 19** standalone components
- ✅ **Auto-refresh** every 10 seconds using RxJS
- ✅ **HttpClient** for API calls
- ✅ **Responsive design**
- ✅ **Real-time metrics**
- ✅ **TypeScript** with strict mode
- ✅ **Error handling**

---

## 📊 Dashboard Metrics

### Displays:

1. Active auctions count
2. Pending payments count
3. Completed auctions count
4. Failed auctions count
5. Total revenue
6. Payment success rate
7. Top 5 bidders
8. Recent 5 auctions

### Auto-updates:

- Refreshes every 10 seconds
- Uses RxJS interval and switchMap
- Unsubscribes on component destroy

---

## 🔧 Configuration

### Change API URL

Edit `src/app/dashboard.service.ts`:

```typescript
private apiUrl = 'http://your-server:8080/api/dashboard';
```

### Change Refresh Interval

Edit `src/app/app.component.ts`:

```typescript
interval(15000); // 15 seconds instead of 10
```

---

## 🛠️ Development

### Install Dependencies

```bash
npm install
```

### Start Dev Server

```bash
npm start
# Opens http://localhost:4200
```

### Build for Production

```bash
npm run build
# Output in dist/bidsphere-dashboard
```

---

## 📦 Dependencies

### Angular 19 Packages:

- @angular/core
- @angular/common
- @angular/platform-browser
- @angular/platform-browser-dynamic
- @angular/animations

### Other:

- RxJS 7.8
- TypeScript 5.6
- Zone.js 0.15

---

## 🧪 Testing

### Test API Connection

```bash
curl http://localhost:8080/api/dashboard
```

### Test Dashboard

1. Start API: `cd BidSphere && dotnet run`
2. Start Angular: `cd frontend && npm start`
3. Open: `http://localhost:4200`
4. Watch metrics update every 10 seconds

---

## 🐛 Troubleshooting

### Error: "npm: command not found"

```bash
# Install Node.js first
# Download from: https://nodejs.org/
```

### Error: "Failed to fetch dashboard data"

```bash
# 1. Check if API is running
curl http://localhost:8080/api/dashboard

# 2. Check CORS is enabled in BidSphere
# 3. Check browser console for errors
```

### Error: Port 4200 already in use

```bash
# Use different port
ng serve --port 4300
```

---

## 🚀 Production Build

```bash
# Build for production
npm run build

# Serve the built files
cd dist/bidsphere-dashboard
python3 -m http.server 8000
```

---

## ✅ Checklist

Before running:

- [ ] Node.js installed (v18+)
- [ ] npm install completed
- [ ] BidSphere API running on port 8080
- [ ] CORS enabled in BidSphere
- [ ] Port 4200 available

---

## 📝 Summary

**Technology:** Angular 19 with standalone components
**Auto-refresh:** Every 10 seconds
**API:** http://localhost:8080/api/dashboard
**Dev Server:** http://localhost:4200
**Build:** npm run build

Enjoy your Angular 19 dashboard! 🎉
