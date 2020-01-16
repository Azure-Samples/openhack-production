FROM node:10.15.0

# make the 'app' folder the current working directory
WORKDIR /app

# copy both 'package.json' and 'package-lock.json' (if available)
COPY package*.json ./

# install project dependencies
RUN npm install
RUN npm install -g @vue/cli

# copy project files and folders to the current working directory (i.e. 'app' folder)
COPY . .

# start app
CMD ["npm", "run", "serve"]