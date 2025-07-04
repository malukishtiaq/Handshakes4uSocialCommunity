# GitHub Actions Workflows

This directory contains organized workflows for building, releasing, and deploying the WoWonder Android app.

## 📁 Folder Structure

### 🔨 `/build/` - Development Builds
- **`android-build.yml`** - Main CI/CD pipeline for development
  - Triggers: Push/PR to main + manual
  - Builds: Debug APK, Release APK, Release AAB
  - Purpose: Continuous integration testing

- **`manual-build.yml`** - Flexible on-demand builds
  - Triggers: Manual dispatch only
  - Choice: APK or AAB format
  - Purpose: Custom builds when needed

- **`simple-android-build.yml`** - Windows-based alternative
  - Triggers: Manual dispatch only
  - Platform: Windows with MSBuild
  - Purpose: Alternative build environment

### 🚀 `/release/` - Production Releases
- **`android-signed-build.yml`** - Signed production builds
  - Triggers: Version tags (v*) + manual
  - Builds: Signed Release APK + AAB
  - Purpose: Production-ready signed releases

### 🌐 `/deploy/` - Store Deployment
- **`deploy-to-play-store.yml`** - Build and deploy to Play Store
  - Triggers: Manual dispatch only
  - Builds: Signed AAB → Upload to Play Console
  - Purpose: Automated Play Store deployment

- **`deploy-existing-aab.yml`** - Deploy pre-built AAB
  - Triggers: Manual dispatch only
  - Uses: Existing AAB from artifacts
  - Purpose: Deploy previously built releases

## 🔄 Workflow Types

| Type | Purpose | When to Use |
|------|---------|-------------|
| **Build** | Create APK/AAB files | Development, testing, CI/CD |
| **Release** | Signed production builds | Creating store-ready files |
| **Deploy** | Upload to Play Store | Publishing to users |

## 🚀 Quick Start

### For Development
1. Use `build/android-build.yml` for regular CI/CD
2. Use `build/manual-build.yml` for custom builds

### For Production
1. Use `release/android-signed-build.yml` to create signed builds
2. Use `deploy/deploy-to-play-store.yml` to upload to Play Store

### For Customer Deployment
1. Setup Google Play API (see deployment guide)
2. Configure GitHub secrets
3. Run deployment workflows

## 📚 Documentation

- **Deployment Guide**: `/docs/deployment/PLAY_STORE_DEPLOYMENT_GUIDE.md`
- **Setup Instructions**: Follow the deployment guide for API setup
- **Troubleshooting**: Check workflow logs and deployment guide

## 🔧 Configuration

### Required Secrets
- `GOOGLE_PLAY_JSON_KEY` - For Play Store deployment
- `KEYSTORE_PASSWORD` - Android keystore password
- `KEY_ALIAS` - Android key alias
- `KEY_PASSWORD` - Android key password

### Input Parameters
Each workflow accepts specific parameters - check individual workflow files for details.

## 🔄 Workflow Dependencies

```
Development Flow:
build/android-build.yml → Testing & Review

Production Flow:
release/android-signed-build.yml → deploy/deploy-to-play-store.yml

Quick Deploy Flow:
deploy/deploy-existing-aab.yml (uses existing artifacts)
``` 