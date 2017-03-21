FROM ulflunde/authentication-sandbox:latest
#RUN apt-get -y update 
WORKDIR /usr/share/nginx/html
COPY dist .
