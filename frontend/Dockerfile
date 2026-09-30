FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
# npm ci fails here: the lock file (generated on Windows) is missing a couple of
# @napi-rs/wasm-runtime peer deps that only resolve on Linux. `npm install` re-resolves
# them for this platform instead of demanding byte-for-byte lock fidelity.
RUN npm install

COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
