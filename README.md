# Fitverse-Web-App

## Google OAuth Login (Authentication Microservice)

The authentication service now supports Google OAuth using the authorization-code flow with PKCE and One Tap ID tokens.

### Configuration

| Setting | Description |
| --- | --- |
| Google:ClientId | Google OAuth client id. Leave blank locally and provide via environment variables (e.g. Google__ClientId). |
| Google:ClientSecret | Google OAuth client secret. Provide through environment variables or secrets. |
| Google:RedirectUri | Callback URL served by the auth service, for example http://localhost:5173/api/auth/google/callback. |
| Google:AuthorizationUri | Authorization endpoint (defaults to https://accounts.google.com/o/oauth2/v2/auth). |
| Google:TokenUri | Token endpoint (defaults to https://oauth2.googleapis.com/token). |
| Google:UserInfoUri | Userinfo endpoint (defaults to https://openidconnect.googleapis.com/v1/userinfo). |
| Google:Scopes | Array of scopes requested during consent (openid profile email by default). |
| Google:StateTtlMinutes | TTL for stored OAuth state/code-verifier values (default 5 minutes). |
| Google:CodeVerifierLength | Length of generated PKCE code verifier (default 64). |

For production deployments (see docker-compose-production.yml), inject these values with environment variables, e.g.

`
Google__ClientId=
Google__ClientSecret=
Google__RedirectUri=
Google__AuthorizationUri=https://accounts.google.com/o/oauth2/v2/auth
Google__TokenUri=https://oauth2.googleapis.com/token
Google__UserInfoUri=https://openidconnect.googleapis.com/v1/userinfo
Google__Scopes__0=openid
Google__Scopes__1=profile
Google__Scopes__2=email
Google__StateTtlMinutes=5
Google__CodeVerifierLength=64
`

Ensure the same values are supplied to the gateway/frontend when constructing the authorization URL.

### Bookkeeping Tables

A new external_login table stores provider mappings (provider, provider_user_id) tied to local users. EF Core migration 20250930153624_AddExternalLogins seeds the schema with unique constraints and indexes.

### API Endpoints

* GET /api/auth/v1.0/google/url?redirectUri= – returns a signed-in authorization URL and state for PKCE flows.
* GET /api/auth/v1.0/google/callback?code&state – exchanges code for tokens and issues Fitverse JWT/refresh tokens.
* POST /api/auth/v1.0/login/google – accepts { code, state, redirectUri } payload for server-to-server exchanges.
* POST /api/auth/v1.0/google/verify-id-token – accepts { idToken } for Google One Tap or Sign-In button flows.

All routes respect linking rules:

1. Existing mapping (provider + provider_user_id) signs in immediately.
2. Verified email with existing user links the Google account.
3. Unverified email cannot auto-link to an existing user.
4. Missing email creates a new active customer user and mapping.
5. Inactive users are blocked even if a mapping exists.
6. LastLoginAt is updated on successful sign-in; email confirmation and profile gaps are back-filled when available.

Refer to LoginWithGoogleCommandHandler tests for acceptance scenarios.

### Testing

Run the authentication microservice test suite (includes Google login unit tests and architecture rules):

`
dotnet test Microservices/Authentication.Microservice/test/test.csproj
`
