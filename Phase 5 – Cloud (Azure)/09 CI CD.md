# CI/CD

## Table of content

## What is CI/CD?

> CI/CD = Continuous Integration / Continuous Delivery (or Deployment)

## CI — Continuous Integration

CI is about **automatically building and testing code whenever developers push changes**.

Typical flow:

```text
Developer pushes code
 |
Build
 |
Unit tests
 |
Integration tests
 |
Code quality/security checks
 |
Artifact created
```

The goal is to catch problems **early**, before code gets merged or released.

Example with a .NET API:

```text
git push
 |
Azure DevOps / GitHub Actions
 |
dotnet restore
 |
dotnet build
 |
dotnet test
 |
Docker image / application artifact
```

## CD — Continuous Delivery

**Continuous Delivery** means the application is automatically prepared and deployed to an environment, but **production deployment usually requires a manual approval**.

```text
CI
 |
Build artifact
 |
Deploy to Dev
 |
Deploy to Test
 |
Deploy to Staging
 |
Manual approval
 |
Production
```

The important idea is:

> **The software is always in a releasable state.**

## CD — Continuous Deployment

Continuous Deployment goes one step further.

After automated tests and checks pass, the system **automatically deploys to production**.

```text
Code
 |
Build
 |
Test
 |
Security checks
 |
Deploy Dev
 |
Deploy Test
 |
Deploy Production
```

No manual production approval.

## Easy way to remember

|                      | CI               | Continuous Delivery     | Continuous Deployment |
| -------------------- | ---------------- | ----------------------- | --------------------- |
| Build automatically  | ✅               | ✅                      | ✅                    |
| Test automatically   | ✅               | ✅                      | ✅                    |
| Deploy automatically | ❌               | ✅                      | ✅                    |
| Production approval  | N/A              | Usually ✅              | ❌                    |
| Main goal            | Catch bugs early | Always ready to release | Release automatically |

# Interview Tips

> **CI is about continuously integrating code changes and automatically building and testing them. CD takes the validated artifact and automates its delivery through environments. With Continuous Delivery, production usually has a manual approval step, whereas Continuous Deployment automatically releases successful changes to production.**

For example, in **Azure DevOps**, you might have a CI pipeline that builds and tests your .NET application and publishes a Docker image, followed by a CD pipeline that deploys that image to **Azure App Service or Kubernetes**.

If asked **"What's the difference between CI and CD?"**, don't just say "CI is build, CD is deploy." Mention the key distinction:

**CI → integrate + build + test**
**CD → deliver/deploy the validated artifact**
**Continuous Delivery → production approval**
**Continuous Deployment → fully automatic production release**
