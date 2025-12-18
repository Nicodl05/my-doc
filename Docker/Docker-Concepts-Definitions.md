# Docker — Définitions détaillées des concepts

But: fournir des définitions complètes et pratiques des principaux concepts Docker pour préparation d'entretien.

1) Docker Engine / dockerd
- Définition: service qui orchestre la création, exécution et gestion des containers. `dockerd` travaille avec containerd et runc pour démarrer les processus.

2) Image
- Définition: artefact immuable construit à partir d'un `Dockerfile`. Contient couches (layers) empilées et métadonnées (entrypoint, env, labels).
- Layer: chaque instruction `RUN`, `COPY`, `ADD` crée une nouvelle couche; les couches sont mises en cache et réutilisées pour optimiser les rebuilds.

3) Container
- Définition: instance en cours d'exécution d'une image ; compose un processus isolé avec son writable layer.

4) Dockerfile (instructions clés)
- `FROM`: image de base; multi-stage builds utilisent plusieurs `FROM`.
- `RUN`: exécute commande pendant la construction -> crée une couche.
- `CMD`: valeurs par défaut pour le conteneur (arguments) ; peut être remplacé par `docker run <cmd>`.
- `ENTRYPOINT`: commande fixe du conteneur; combine souvent `ENTRYPOINT`+`CMD` pour paramètres par défaut.
- `COPY` vs `ADD`: `COPY` copie fichiers, `ADD` supporte archives et URLs.
- `ENV`, `WORKDIR`, `USER`, `EXPOSE`, `VOLUME`, `HEALTHCHECK`.

5) BuildKit
- Définition: moteur de build amélioré (concurrent, cache avancé, secrets pour build). Active via `DOCKER_BUILDKIT=1`.

6) Registry
- Définition: service servant d'entrepôt d'images (Docker Hub, GHCR, private registries). Opérations: `docker push`, `docker pull`, `docker login`.

7) Tags & Digests
- Tag: étiquette lisible (ex: `:latest`, `:v1.2`).
- Digest: identifiant immuable (sha256) garantissant contenu exact de l'image.

8) Volumes & bind mounts
- Bind mount: mappe répertoire hôte dans conteneur (utile en dev).
- Named volume: géré par Docker; recommandé pour persistance en production.
- `VOLUME` instruction marque un mount point dans Dockerfile.

9) Network drivers
- `bridge`: réseau par défaut pour containers sur le même hôte.
- `host`: partage network namespace de l'hôte (pas d'isolation).
- `overlay`: réseau multi-host (Swarm/compose with swarm mode).

10) Docker Compose
- Outil déclaratif pour définir et lancer multi-container apps via `docker-compose.yml`.

11) Healthcheck
- Définition: instruction Dockerfile `HEALTHCHECK` qui définit comment Docker vérifie la santé du container (commande, intervalle, retries). `docker ps` affiche `healthy/unhealthy`.

12) ENTRYPOINT vs CMD
- `ENTRYPOINT`: définit le processus principal; `CMD`: arguments par défaut. Si les deux sont présents, `CMD` fournit les arguments à `ENTRYPOINT`.

13) Multi-stage builds
- Définition: utiliser plusieurs `FROM` pour séparer build-time et runtime afin de réduire taille finale.

14) OCI, containerd, runc
- Standards et composants: OCI image spec et runtime; `containerd` gère images et lifecycle; `runc` exécute le conteneur.

15) Security (pratiques)
- Exécuter `USER` non-root, utiliser seccomp profiles, restreindre capabilities (`--cap-drop`), read-only root filesystem (`--read-only`), scanner images (Trivy), signer images (cosign).

16) Docker contexts
- Définition: mécanisme pour définir endpoints Docker (local, remote, ssh) et switcher entre eux (`docker context use`).

17) Docker Swarm (aperçu)
- Orchestrateur intégré à Docker (services, stacks, routing mesh). Swarm est moins courant que Kubernetes mais utile à connaître.

18) Resource limits
- Limiter CPU/memory (`--memory`, `--cpus`, `--memory-swap`) pour éviter saturation hôte.

19) Docker Hub rate limits & auth
- Connaître limites de pull, politiques d'auth et pratiques pour utiliser registres privés si nécessaire.

20) Troubleshooting common issues
- Image pull auth failure: vérifier `docker login` and registry credentials.
- Permission errors on bind mounts: vérifier UID/GID and file permissions on host.
- Port conflicts: vérifier `docker ps` et hôtes utilisant même port.

---

Exemples rapides:

Simple Dockerfile:
```
FROM node:18-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
EXPOSE 3000
CMD ["node", "server.js"]
```

Multi-stage Dockerfile (Go):
```
FROM golang:1.20-alpine AS builder
WORKDIR /src
COPY . .
RUN CGO_ENABLED=0 GOOS=linux go build -o app ./cmd/app

FROM gcr.io/distroless/static:nonroot
COPY --from=builder /src/app /app/app
USER nonroot
ENTRYPOINT ["/app/app"]
```

docker-compose.yml minimal:
```
version: '3.8'
services:
  web:
    build: .
    ports:
      - "8080:80"
    volumes:
      - .:/app:delegated
  db:
    image: postgres:15
    volumes:
      - db-data:/var/lib/postgresql/data
volumes:
  db-data:
```

Healthcheck example:
```
HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD curl -f http://localhost/healthz || exit 1
```
