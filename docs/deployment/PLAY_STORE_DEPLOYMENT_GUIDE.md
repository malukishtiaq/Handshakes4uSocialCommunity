# Google Play Store Deployment Guide

This guide explains how to set up automated deployment to Google Play Console using GitHub Actions.

## Overview

The `deploy-to-play-store.yml` workflow automates the process of building and uploading your AAB file to Google Play Console using the Google Play Developer API.

## Prerequisites

### 1. Customer Account Setup

The customer (Play Console account owner) needs to:

1. **Create a Google Cloud Project**
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or use existing one

2. **Enable Google Play Developer API**
   - In Google Cloud Console, go to "APIs & Services" > "Library"
   - Search for "Google Play Developer API"
   - Click "Enable"

3. **Create Service Account**
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "Service Account"
   - Name it (e.g., "GitHub Actions Deployment")
   - Click "Create and Continue"
   - Skip role assignment for now
   - Click "Done"

4. **Generate Service Account Key**
   - Click on the created service account
   - Go to "Keys" tab
   - Click "Add Key" > "Create new key"
   - Choose "JSON" format
   - Download the JSON file

5. **Grant Play Console Access**
   - Go to [Play Console](https://play.google.com/console)
   - Go to "Setup" > "API access"
   - Find your service account and click "Grant access"
   - Set permissions:
     - ✅ View app information and download bulk reports
     - ✅ Manage store presence
     - ✅ Manage app releases
   - Click "Apply"

### 2. GitHub Repository Setup

#### Required Secrets

Add these secrets to your GitHub repository (`Settings` > `Secrets and variables` > `Actions`):

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `GOOGLE_PLAY_JSON_KEY` | Service account JSON content | `{"type": "service_account", ...}` |
| `KEYSTORE_PASSWORD` | Android keystore password | `wowonder` |
| `KEY_ALIAS` | Android key alias | `wowondertimelinechat` |
| `KEY_PASSWORD` | Android key password | `wowonder` |

#### Setting up GOOGLE_PLAY_JSON_KEY

1. Open the downloaded JSON file
2. Copy the **entire** JSON content
3. Paste it as the value for `GOOGLE_PLAY_JSON_KEY` secret

## Usage

### Running the Workflow

1. Go to your repository on GitHub
2. Click "Actions" tab
3. Select "Deploy to Google Play Store" workflow
4. Click "Run workflow"
5. Fill in the required inputs:

| Input | Description | Example |
|-------|-------------|---------|
| **Release Track** | Where to deploy | `internal`, `alpha`, `beta`, `production` |
| **Developer Account ID** | From Play Console URL | `5815054519387382632` |
| **Package Name** | Your app's package name | `com.yourcompany.wowonder` |

### Finding Your Information

#### Developer Account ID
From the Play Console URL: `https://play.google.com/console/u/0/developers/[ACCOUNT_ID]`

#### Package Name
- Found in your `AndroidManifest.xml`
- Or in Play Console under "App content" > "App details"

## Release Tracks Explained

| Track | Purpose | Who Can Access |
|-------|---------|----------------|
| **Internal** | Team testing | Up to 100 internal testers |
| **Alpha** | Closed testing | Selected users only |
| **Beta** | Open/Closed testing | Wider testing group |
| **Production** | Live release | All users |

## Workflow Features

### What It Does
1. ✅ Builds signed AAB file
2. ✅ Uploads to specified track
3. ✅ Validates package name
4. ✅ Provides deployment summary
5. ✅ Saves AAB as artifact

### What It Doesn't Do
- ❌ Publish automatically (requires manual approval in Play Console)
- ❌ Upload metadata/screenshots
- ❌ Update store listing

## Troubleshooting

### Common Issues

#### "Package not found" Error
- ✅ Verify package name matches exactly
- ✅ Ensure app exists in Play Console
- ✅ Check service account has access to this app

#### "Insufficient permissions" Error
- ✅ Verify service account has correct permissions
- ✅ Check API is enabled in Google Cloud
- ✅ Ensure service account is linked in Play Console

#### "Invalid JSON key" Error
- ✅ Copy entire JSON content including braces
- ✅ Don't modify the JSON content
- ✅ Ensure no extra spaces/characters

#### "APK/AAB already exists" Error
- ✅ Increment version code in your project
- ✅ Use different version for each upload

### Getting More Details

Check the workflow logs for detailed error messages:
1. Go to Actions tab
2. Click on the failed workflow run
3. Expand the "Deploy to Play Store" step

## Security Best Practices

### Service Account Security
- 🔒 Keep JSON key secure
- 🔒 Use GitHub Secrets (never commit keys)
- 🔒 Limit service account permissions
- 🔒 Regularly rotate keys if needed

### Repository Security
- 🔒 Limit who can run workflows
- 🔒 Use branch protection rules
- 🔒 Review deployment logs

## Manual Deployment Alternative

If automated deployment isn't working, you can:

1. Run the build-only workflows
2. Download AAB from artifacts
3. Upload manually to Play Console

## Support

For issues with:
- **Workflow**: Check GitHub Actions logs
- **Google Play API**: Check [Google Play Console Help](https://support.google.com/googleplay/android-developer)
- **Fastlane**: Check [Fastlane Documentation](https://docs.fastlane.tools/) 