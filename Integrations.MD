# Nginx

add_authica_endpoint.conf
```
set $upstream_authica http://authica.example.com/nginx-verify;

# Virtual endpoint created by nginx to forward auth requests.
location /authica {
    internal;
    proxy_pass_request_body off;
    proxy_pass $upstream_authica;
    proxy_set_header Content-Length "";

    # Timeout if the real server is dead
    proxy_next_upstream error timeout invalid_header http_500 http_502 http_503;

    # [REQUIRED] Needed to check authorizations of the resource.
    # Provide either X-Original-URL and X-Forwarded-Proto or
    # X-Forwarded-Proto, X-Forwarded-Host and X-Forwarded-Uri or both.
    # Those headers will be used to deduce the target url of the user.
    # Basic Proxy Config
    client_body_buffer_size 128k;
    proxy_set_header Host $host;
    proxy_set_header X-Original-URL $scheme://$http_host$request_uri;
    proxy_set_header X-Forwarded-For $remote_addr;
    proxy_redirect  http://  $scheme://;
    proxy_http_version 1.1;
    proxy_set_header Connection "";
    proxy_cache_bypass $cookie_session;
    proxy_no_cache $cookie_session;
    proxy_buffers 4 32k;

    # Advanced Proxy Config
    send_timeout 5m;
    proxy_read_timeout 240;
    proxy_send_timeout 240;
    proxy_connect_timeout 240;
}
```

secure_with_authica.conf
```
# Basic Config
# Send a subsequent request to verify if the user is authenticated
# and has the right permissions to access the resource.
auth_request /authica;
# Set the `target_url` variable based on the request. It will be used to build the portal
# URL with the correct redirection parameter.
auth_request_set $target_url $scheme://$http_host$request_uri;

# Set the X-Forwarded-User and X-Forwarded-Groups with the headers
# returned for the backends which can consume them.
# This is not safe, as the backend must make sure that they come from the
# proxy. In the future, it's gonna be safe to just use OAuth.
#auth_request_set $user $upstream_http_remote_user;
#auth_request_set $groups $upstream_http_remote_groups;
#auth_request_set $name $upstream_http_remote_name;
#auth_request_set $email $upstream_http_remote_email;
#proxy_set_header Remote-User $user;
#proxy_set_header Remote-Groups $groups;
#proxy_set_header Remote-Name $name;
#proxy_set_header Remote-Email $email;

# If it returns 401, 403 or 404 then nginx redirects the user to the login portal.
# If it returns 200, then the request pass through to the backend.
# For other type of errors, nginx will handle them as usual.
error_page 401 403 404 =302 https://auth-test.ica.hr/authorize?rd=$target_url;
```