## Who are we and what are we building?

Join the team behind the operational backbone of a leading commodity-intelligence company. Our platform is the internal tool analysts use to model mining, metals and fertilizerassets - cost curves, per-asset cost data across aluminium, lithium, base metals, fertilizers, steel and gold, capex, macroeconomic drivers, and data quality. The data we produce powers the company's downstream analytical products. We pushcurated cost and asset data into the analytical warehouse via Azure Service Bus and ETLfunctions - relied on by traders, strategists and analysts at top commodity producers, banksand consultancies for decisions worth billions of dollars. The platform is mature and business-critical - collaborative, data-quality-driven, and being modernised into a more performant, observable, AI-augmented backend.

## Where are we now?

The platform is a live, business-critical product. The backend is .NET 8 / ASP.NET Core (Web API + MVC) covering assets, costs, capex, scenarios and data quality, hosted onAzure PaaS - App Service, Functions, Service Bus, Key Vault, Storage, Application Insights. Data lives in MongoDB. ETL runs through Azure Functions and a TypeScript Nx monorepo. SignalR powers realtime collaboration, Polly handles resilience, Entra ID handles auth. The Azure footprint is fully Terraform-managed; CI runs on GitHub Actions. We have a working product, a clear roadmap, and an engaged user base. Now we arescaling the team to:

- Ship more business value - new asset, cost, emissions and scenario features.
- Strengthen our data pipelines from operational stores into the analytical warehouse.
- Raise the bar on performance, observability and enterprise readiness.
- Turn AI tooling (Cursor, Claude, Copilot) into a real engineering force multiplier across the team.

## Your Mission (Key Challenges)

We are looking for a Mid Backend .NET Developer who is energised by building production-grade backends on Azure, who already feels at home in Terraform-managed cloud infrastructure, and who is curious to grow into the ETL side of the platform.

- Backend feature delivery. You will own features end-to-end inside our ASP.NET Core 8 backend - from a new database query, through a domain service, to a clean REST endpoint. Azure & Infrastructure as Code. You will not just consume the cloud - you will help own it. Extending Terraform modules, wiring app settings via Key Vault references, configuring Service Bus queues, networking and Front Door rules.
- Data pipelines (with room to grow). Our backend is tightly tied to data flows from MongoDB through Azure Service Bus into the Azure SQL warehouse, plus a TypeScript Nx ETL mono-repo. You don't need to arrive as an ETL expert - but you do need to be open and curious about that world, and willing to learn idempotency, retries, dead-lettering and reconciliation as you go.
- Business-logic discovery. The biggest challenge in this product is not the framework - it's the domain. Assets, commodities, cost stages, emissions, capex, valuations, permissions… You will explore the database, navigate the platform and map both, turning requirements into clear, testable features.
- AI-enabled engineering. We already use Cursor rules and Claude skills as part of our daily workflow. We expect you to be a confident AI-tool user, contribute to our shared rules and prompts, and help shape how the team uses AI well - without losing engineering judgement.
- Quality & enterprise readiness. The platform serves enterprise customers. You will write code with testing, security and performance in mind, and contribute to the ongoing work of raising our enterprise-readiness bar.

## Why join us?

- Internal tool, outsized impact. This is an internal platform - but the data it produces feeds the company's commercial analytical products and ends up in the hands of traders, strategists and analysts at top-tier commodity, mining and finance organisations.
- Domain that rewards curiosity. Commodities, energy transition, asset economics - a rich domain where understanding the business makes you a dramatically better engineer.
- Modern stack, modern practices. .NET 8, ASP.NET Core, **EF Core**, **Dapper**, **MongoDB**, Azure PaaS, **Terraform**, **Datadog** - and a team that actively invests in Cursor rules, Claude skills and AI ways of working.
- Path to direct cooperation. The role starts as a B2B engagement via our partner agency, with a planned transition to a direct contract after 3 months

## Requirements

### Must-have

- C#/.NET - 3+ years of professional backend experience, currently working with .NET 6/7/8 and ASP.NET Core.
- Document data - practical experience with **MongoDB** from .NET.
- Azure PaaS - production experience with App Service, Azure Functions, Service Bus, Key Vault, Storage and Application Insights.
- **Terraform** in production - modules, multi-environment setups, state management, providers and data sources. Experience with Terraform Cloud / TFE or a similar remote-state setup is also expected.
- Openness to ETL / data work - you don't need prior ETL experience, but you must be willing and curious to grow into it.
- Testing discipline - writes unit tests by default, understands integration vs unit trade-offs.
- Domain curiosity - when given a request, your instinct is to open the database, run queries and click through the UI to confirm assumptions, not to wait for a perfect ticket.
- AI-enabled developer - daily user of at least one modern AI coding tool (Cursor, Claude Code, Copilot…); able to discuss what works, what doesn't, and how to set up rules / context for a team.
- Languages - Polish (fluent / native, for daily team comms) and English C1 (all code, PRs, Jira, Confluence and cross-team comms are in English).
- Communication & proactivity - comfortable in an async-first distributed team, with strong written communication.

### Nice-to-have

- TypeScript / Node.js good enough to work on our Nx ETL monorepo and Node Azure Function (Prisma, mssql, mongodb drivers).
- Hands-on ETL experience - you've already shipped data import / export / migration / reconciliation flows in production, with idempotency, dry-runs and bulk updates.
- React + TypeScript frontend - able to pick up a frontend ticket when needed. Our client is React 19 + TypeScript on Vite. You won't own the frontend, but being able to ship a small UI change end-to-end alongside your API work is a real plus.
- Cloud networking - VNets, subnets, private endpoints, private DNS, NSGs, VNet-integrated apps and functions.
- Messaging patterns - queues / topics, retries, poison messages, at-least-once semantics, ideally on Azure Service Bus.
- Docker and containerized App Service deployments (multi-stage builds, ACR).
- Azure Front Door / WAF, CDN.
- Datadog and / or Application Insights, log correlation, custom JSON logging.
- CI / security tooling - Azure DevOps Pipelines, GitHub Actions, Trivy IaC scanning.
- Excel / CSV / MDB ingestion (ClosedXML, xlsx, exceljs, papaparse, mdb-reader).

### What we will NOT expect from you

- Designing the analytical warehouse schema or the broader data model from scratch - owned by our Data Engineering Team.
- ML / AI model training or data engineering pipelines outside the app.
- Deep DevOps / SRE on-call work - we have infra owners and a working Terraform setup; we want a partner, not a one-person platform team.
