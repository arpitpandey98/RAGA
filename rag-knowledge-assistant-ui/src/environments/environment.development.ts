export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080',
 // if you are using IIS Express, you can use the following URL instead of localhost:8080
 // apiUrl: 'http://localhost:44300',
  auth: {
    clientId: 'b24f6527-1c97-42b7-a5c7-4f61221e47fe',
    tenantId: 'dc5be81c-b325-4dc0-9920-709a5db81d0c',
    apiClientId: 'e4040e09-1b92-4b90-a5ae-330b407e1ac0',
    apiScope: 'access_as_user',
    authority: 'https://ragacustomers.ciamlogin.com/dc5be81c-b325-4dc0-9920-709a5db81d0c/'
  }
};