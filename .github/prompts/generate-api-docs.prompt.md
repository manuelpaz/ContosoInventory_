 ---
 description: 'Generate API documentation for the active controller file'
 agent: 'copilot'
 tools: ['search', 'read_file']
 ---
 # Generate API Documentation

 Analyze the controller in ${file} and generate comprehensive API documentation.

 Follow the project conventions in the controller and service instructions:
 - keep the documentation focused on public API behavior
 - describe DTO-based request and response payloads
 - document controller actions as thin HTTP handlers that delegate business logic to services

 For each endpoint in the controller:
 1. List the HTTP method and route.
 2. Describe the purpose of the endpoint.
 3. Document the request parameters, body, and query string inputs.
 4. Document the possible response status codes and their meanings.
 5. Provide an example request and response in JSON format.

 Format the output as a Markdown document suitable for a developer wiki.
