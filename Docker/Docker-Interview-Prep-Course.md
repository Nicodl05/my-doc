# Cours Complet Docker — Préparation Entretien Technique

Version: 1.0

Remarque: Ce cours est conçu pour préparer des entretiens techniques couvrant Docker (concepts, pratique, sécurité, CI/CD). Il inclut définitions, commandes essentielles, scénarios pratiques, exercices et questions d'entretien.

Assumptions:
- Connaissances de base en systèmes Linux et concepts de conteneurisation.
- Environnement recommandé: Docker Desktop ou Docker Engine (Linux), `docker` et `docker-compose` installés.

Plan du cours
- Introduction & Glossaire
- Images, Containers, Layers, Union FS
- Dockerfile: instructions et bonnes pratiques
- Build, tag, push: registries (Docker Hub, private registry)
- Volumes, bind mounts, tmpfs
- Networking: bridge, host, overlay, publish ports
- Docker Compose: services, volumes, networks, profiles
- Healthchecks, entrypoint vs CMD
- Multi-stage builds et optimisation d'images
- Security: least privilege, user, namespaces, seccomp, capabilities, image scanning
- CI/CD: building images in pipelines, caching, registries, signed images
- Troubleshooting & Debugging
- Questions d'entretien + réponses
- Labs pratiques, exemples Dockerfiles & docker-compose
- Cheat-sheet `docker` & `docker-compose`

=================================================================

**Introduction & Glossaire**

- Image: paquet immuable contenant filesystem et métadonnées (créée via Dockerfile).
- Container: instance en cours d'exécution d'une image; processus isolé.
- Layer: couche immuable dans une image; les couches sont mises en cache et réutilisées.
- Registry: service hébergeant les images (Docker Hub, GitHub Container Registry, Harbor).

=================================================================

**Module 1 — Images, containers, layers**

Concepts:
- Images construites en couches; chaque instruction Dockerfile crée une couche.
- Union FS (overlay2, aufs) compose le système de fichiers d'un container.
- Différence image vs container: l'image est statique; le container a un état éphémère (Writable layer).

Commandes clés:
```
docker images
docker pull nginx:latest
docker run --rm -d --name web nginx:latest
docker ps -a
docker inspect <container>
docker logs <container>
```

=================================================================

**Module 2 — Dockerfile & bonnes pratiques**

Instructions principales: `FROM`, `RUN`, `COPY`, `ADD`, `CMD`, `ENTRYPOINT`, `ENV`, `EXPOSE`, `VOLUME`, `WORKDIR`, `USER`, `HEALTHCHECK`.

Bonnes pratiques:
- Utiliser des images officielles minimales (alpine/distroless) si possible.
- Préférer `COPY` à `ADD` sauf besoin d'extraction ou URL.
- Multi-stage builds pour réduire la taille finale.
- Minimiser le nombre de couches combinant commandes `RUN` (utiliser `&&` et cleanup).
- Utiliser `.dockerignore` pour exclure fichiers inutiles.

=================================================================

**Module 3 — Volumes & stockage**

- Bind mount: mappe dossier hôte dans container (développement), potentiellement non portable.
- Named volume: géré par Docker, persiste données et isolé du host path.
- tmpfs: stockage en mémoire.

Commandes:
```
docker volume create mydata
docker run -v mydata:/var/lib/data ...
docker run -v $(pwd):/app ...
```

=================================================================

**Module 4 — Networking**

- Drivers: bridge (default), host, overlay (multi-host via Swarm), macvlan.
- Publishing ports: `-p hostPort:containerPort`.
- Linking deprecated; use networks and DNS service discovery in compose.

Commandes:
```
docker network ls
docker network create mynet
docker run --network mynet --name app ...
```

=================================================================

**Module 5 — Docker Compose**

- Définir multi-container apps via `docker-compose.yml` (services, volumes, networks, depends_on).
- Profiles, env_file, build context.

Commandes:
```
docker compose up -d
docker compose logs -f
docker compose down --volumes
```

=================================================================

**Module 6 — Healthcheck, ENTRYPOINT vs CMD**

- `HEALTHCHECK` dans Dockerfile permet à Docker de signaler l'état de santé d'un container.
- `ENTRYPOINT` fixe le binaire principal; `CMD` fournit arguments par défaut. Utiliser `ENTRYPOINT` pour wrapper inamovible.

=================================================================

**Module 7 — Multi-stage builds & optimisation**

- Utiliser un stage builder (compilation) puis copier les artefacts dans une image runtime légère.
- Exploiter cache du build (ordre des instructions, `--cache-from`).

=================================================================

**Module 8 — Sécurité**

- Exécuter non-root (`USER`), limiter capabilities, activer seccomp profiles, utiliser read-only filesystem si possible.
- Scanner images (Trivy, Clair), signer images (Notary/Notary v2, cosign).
- Ne pas stocker secrets dans images; utiliser secrets manager ou `--mount=type=secret` (Docker Swarm / BuildKit secrets).

=================================================================

**Module 9 — CI/CD & registries**

- Meilleures pratiques: build reproducible, tagged images (use digest in deploy), push to private registry.
- Use BuildKit for faster builds + secrets during build.

=================================================================

**Module 10 — Troubleshooting**

Checklist:
1. `docker ps -a`, `docker logs`.
2. `docker inspect` (networking, mounts, env).
3. `docker exec -it` pour debug.
4. `docker stats` for resource usage.

Scénarios: crash on start (entrypoint), port conflicts, permission errors on volumes, image pull auth.

=================================================================

**Module 11 — Questions d'entretien (sélection)**

Q1: Quelle différence entre `COPY` et `ADD` ?
A1: `COPY` copie fichiers locaux; `ADD` peut extraire archives tar et accepter URL — préférez `COPY` sauf besoin.

Q2: Pourquoi utiliser multi-stage builds ?
A2: Séparer phase build (compilation) et runtime, réduire taille image finale et éviter d'inclure outils de build.

Q3: Comment sécuriser une image Docker ?
A3: Exécuter non-root, appliquer seccomp/capabilities, scanner images, signer, éviter secrets dans image.

=================================================================

**Module 12 — Labs pratiques & exemples**

Lab 1: Construire une application Go en multi-stage et exécuter l'image.
Lab 2: Composer une stack (web + db + redis) avec `docker-compose` et persister volumes.
Lab 3: Simuler une panne et debugger (logs, exec, inspect).

=================================================================

**Module 13 — Cheat-sheet `docker` & `docker compose`**

- `docker build -t myapp:latest .`
- `docker run --rm -d -p 8080:80 --name web myapp:latest`
- `docker ps -a`, `docker logs -f <container>`
- `docker images`, `docker rmi <image>`
- `docker compose up -d`, `docker compose down --volumes`

=================================================================

**Questions d'entretien — Solutions complètes**

Pour révision rapide, voici les questions clés avec réponses développées (liste complète et classée dans `Docker/Docker-Questions.md`).

Junior
- Q: Qu'est-ce qu'une image Docker ?
	A: Une image est un artefact immuable créé à partir d'un Dockerfile; elle est constituée de couches (layers) représentant chaque étape du build, stockée dans un registry et utilisée comme base pour lancer des containers.

- Q: Qu'est-ce qu'un container ?
	A: Processus isolé créé à partir d'une image, avec son propre writable layer, namespaces (PID, NET, MNT, IPC, UTS) et cgroups pour la gestion des ressources.

- Q: Différence entre `CMD` et `ENTRYPOINT` ?
	A: `ENTRYPOINT` définit le programme principal du conteneur; `CMD` fournit des arguments par défaut qui peuvent être surchargés par `docker run`. Si `ENTRYPOINT` est absent, `CMD` est la commande par défaut.

- Q: Pourquoi utiliser `.dockerignore` ?
	A: Pour exclure fichiers/dossiers du contexte de build afin de réduire le build context envoyé au daemon et améliorer vitesse/taille de l'image.

Intermédiaire
- Q: Qu'est-ce qu'une couche (layer) dans une image ?
	A: C'est l'état du filesystem produit par une instruction Dockerfile. Les couches sont immuables et réutilisables (cache), ce qui accélère les builds.

- Q: Comment réduire la taille d'une image ?
	A: Utiliser multi-stage builds, choisir images de base légères (alpine/distroless), combiner commandes RUN pour réduire couches, nettoyer caches (apt-get clean), et exclure fichiers via `.dockerignore`.

- Q: Quelle est la différence entre bind mount et named volume ?
	A: Bind mount mappe un chemin exact du host dans le conteneur (développement); named volume est géré par Docker, persiste indépendamment du host path et est plus portable.

Senior
- Q: Comment sécuriser l'exécution d'un container en production ?
	A: Exécuter en non-root, limiter capabilities (`--cap-drop`), appliquer un profil seccomp/AppArmor, monter filesystem readonly si possible, limiter ressources via `--memory`/`--cpus`, scanner et signer images, et séparer secrets hors de l'image.

- Q: Expliquez multi-stage build et ses bénéfices.
	A: Permet d'utiliser un conteneur builder pour compiler, puis copier seulement l'artefact final dans une image runtime légère, réduisant la taille et la surface d'attaque.

=================================================================