# Azure Deployment Checklist

## 1) Deploy the Web App (Azure App Service)
1. Create a resource group.
2. Create an App Service plan.
3. Create a Web App (Linux or Windows).
4. Download the publish profile and add it to GitHub secrets as `AZURE_WEBAPP_PUBLISH_PROFILE`.
5. Set the app name secret as `AZURE_WEBAPP_NAME`.
6. Ensure the workflow `.github/workflows/azure-webapp.yml` is enabled.

## 2) Deploy the Database (Azure Database for PostgreSQL)
1. Create Azure Database for PostgreSQL Flexible Server.
2. Create the database (e.g., `PetSearchHome`).
3. Open firewall to allow the App Service outbound IPs.
4. Obtain the connection string.

## 3) Store Secrets in Azure Key Vault
1. Create a Key Vault.
2. Add secrets:
   - `ConnectionStrings--DefaultConnection`
   - `Geocoding--ApiKey`
   - `JwtSettings--Secret`
3. Enable a managed identity for the App Service.
4. Grant Key Vault `Get` and `List` permissions to the managed identity.
5. Add app setting `KeyVault__Uri` with your vault URI.

## 4) CI/CD with GitHub Actions
- Workflow: `.github/workflows/azure-webapp.yml`
- Includes build + test + publish + deploy.

## 5) SonarCloud and Branch Protection
1. Create a SonarCloud project and token.
2. Add GitHub secrets:
   - `SONAR_TOKEN`
   - `SONAR_ORGANIZATION`
   - `SONAR_PROJECT_KEY`
3. Enable workflow `.github/workflows/sonarcloud.yml`.
4. In GitHub repo settings, configure branch protection rules:
   - Require a pull request before merging.
   - Require at least 1 approval.
   - Require status checks to pass (Azure workflow + SonarCloud).
   - Restrict direct pushes to the main branch.
