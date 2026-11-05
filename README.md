# AEPW Impact Metrics Tracking (IMT) Portal

## Overview
The **AEPW Impact Metrics Tracking (IMT) Portal** is a web-based platform designed to replace the current spreadsheet- and Salesforce-driven process used by AEPW for collecting, validating, approving, and reporting impact metrics across its global portfolio.

This solution enables internal users to manage quarterly review cycles, upload and validate evidence, apply risk adjustments, route approvals dynamically, and generate portfolio-level impact reports with full traceability and auditability.

This repository contains the source code, configuration, and supporting assets for the IMT Portal delivery.

---

## Business Context
AEPW currently manages impact reporting using a combination of:
- Salesforce
- Offline Word/Excel forms
- Email-based evidence exchange
- SharePoint folders

This results in:
- Manual reconciliation of impact data and supporting evidence
- Limited visibility into submission and approval status
- Fragmented evidence storage
- High approval effort for reviewers
- Difficulty in managing restatements and audit trails

The IMT Portal is being built to centralize and streamline this process.

---

## Objectives
The primary objectives of the IMT Portal are to:

- Replace the Salesforce-based impact data collection process for Phase 1 scope
- Reduce manual effort in evidence reconciliation and reporting
- Provide end-to-end traceability from reported numbers to source evidence
- Support structured approval workflows with audit trail
- Enable portfolio-level aggregation, filtering, and reporting
- Migrate historical review data from Salesforce
- Establish a scalable foundation for future external grantee access in Phase 2

---

## Scope

### Phase 1 – In Scope
Phase 1 includes:

- Internal portal for AEPW users
- Review management dashboard and workflow
- Create Review wizard
- Risk adjustment configuration and approval
- Online data entry and Excel upload
- Evidence upload and metadata tagging
- AI/OCR-assisted evidence extraction and validation
- Approval workflow:
  - PM Submission
  - Data Validation
  - Senior Review
  - Final Approval
- Audit trail and restatement support
- Program-level reporting and Excel export
- User management and role-based access
- SSO integration with AEPW identity provider
- Historical data migration from Salesforce
- Training materials and train-the-trainer support

### Phase 2 – Planned
Planned future enhancements include:

- Grantee portal access
- Interactive dashboards
- Self-service metric configuration
- Additional analytics and AI-driven insights

### Out of Scope
- Native mobile applications
- Real-time bi-directional Salesforce sync
- Public reporting portal
- Custom AI/ML model training in Phase 1
- Multi-language UI localization
- Cloud hosting and infrastructure provisioning

---

## Key Features

### Review Management
- Dashboard for in-flight reviews
- Status tracking and progress indicators
- Bulk review launch
- Configurable deadlines and escalation rules

### Data Collection
- Online metric entry with auto-save
- Excel-based submission upload
- Field-level validation
- Hierarchical metric rollups
- Materiality identification (Top 80%)

### Evidence Management
- Upload evidence per metric and site
- File-type tagging and metadata capture
- Validation status tracking
- Drill-down from reported value to source evidence



If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)