# Docker — Questions d'entretien (avec réponses concises)

Objectif: liste de questions classées par niveau (Junior → Senior) avec réponses courtes pour révision rapide.

---

**Junior (fondamentaux)**

Q1: Qu'est-ce qu'une image Docker ?
A1: Artefact immuable construit à partir d'un Dockerfile, composé de couches (layers) et métadonnées.

Q2: Qu'est-ce qu'un container ?
A2: Instance en cours d'exécution d'une image ; processus isolé avec son writable layer.

Q3: Différence entre `CMD` et `ENTRYPOINT` ?
A3: `ENTRYPOINT` fixe le binaire principal, `CMD` fournit des arguments par défaut; `CMD` est remplacé si des arguments sont fournis à `docker run`.

Q4: Pourquoi utiliser `.dockerignore` ?
A4: Exclure fichiers inutiles du contexte de build pour accélérer le build et réduire la taille de l'image.

Q5: Comment lister les containers en cours ?
A5: `docker ps` (avec `-a` pour voir tous les containers).

Q6: Comment inspecter les logs d'un container ?
A6: `docker logs <container>`.

Q7: Qu'est-ce qu'un volume Docker ?
A7: Espace de stockage persistant géré par Docker; survives container recreation.

Q8: Comment build une image locale ?
A8: `docker build -t myimage:tag .`

Q9: Qu'est-ce que `docker run --rm` ?
A9: Lance un container et supprime le container automatiquement à l'arrêt.

Q10: Qu'est-ce que `docker-compose` ?
A10: Outil déclaratif pour définir et exécuter des applications multi-container via `docker-compose.yml`.

---

**Intermédiaire (pratique & optimisation)**

Q11: Qu'est-ce qu'une couche (layer) dans une image ?
A11: Résultat d'une instruction Dockerfile (RUN/COPY/ADD); réutilisées via cache pour accélérer rebuilds.

Q12: Comment réduire la taille d'une image ?
A12: Multi-stage builds, utiliser images minimales (alpine/distroless), nettoyer caches dans RUN, minimiser fichiers copiés.

Q13: Quelle est la différence entre bind mount et named volume ?
A13: Bind mount mappe un chemin du host; named volume est géré par Docker et plus portable.

Q14: Comment définir un healthcheck dans Dockerfile ?
A14: `HEALTHCHECK --interval=30s CMD curl -f http://localhost/healthz || exit 1`.

Q15: Que fait `docker inspect` ?
A15: Fournit la configuration complète et l'état en JSON d'une image/container/network/volume.

Q16: Comment utiliser BuildKit et quels avantages ?
A16: Activer `DOCKER_BUILDKIT=1 docker build`; avantages: builds parallèles, cache amélioré, secrets pour build.

Q17: Comment pousser une image vers un registry privé ?
A17: `docker login registry.example.com` puis `docker tag myimage registry.example.com/myrepo/myimage:tag` et `docker push ...`.

Q18: Qu'est-ce qu'un digest d'image ?
A18: Identifiant immuable sha256 garantissant le contenu exact de l'image (ex: image@sha256:...).

Q19: Quand utiliser `--network host` ?
A19: Pour performance réseau constant et compatibilité avec certains services; perd l'isolation réseau.

Q20: Comment partager un fichier de configuration entre host et container en dev ?
A20: Utiliser un bind mount `-v $(pwd)/config:/app/config`.

---

**Senior (sécurité, CI/CD, scénarios complexes)**

Q21: Comment sécuriser l'exécution d'un container en production ?
A21: Exécuter non-root (`USER`), désactiver capabilities inutiles (`--cap-drop`), appliquer seccomp/AppArmor, read-only rootfs, scanner images (Trivy), signer images (cosign).

Q22: Expliquez multi-stage build et ses bénéfices.
A22: Sépare étapes de build et runtime; permet d'avoir outils de build dans le stage de compilation et une image runtime minimaliste finale, réduisant taille et surface d'attaque.

Q23: Comment gérer secrets dans Docker builds ?
A23: Utiliser BuildKit secrets (`--secret`) pour ne pas laisser les secrets dans les couches; ou external secrets manager pour runtime.

Q24: Comment intégrer scanning d'images dans une pipeline CI ?
A24: Après build, exécuter un scanner (Trivy/Clair) ; bloquer push/deploy si vulnérabilités critiques; stocker rapports.

Q25: Quelle stratégie pour tagger images dans CI ?
A25: Tag par commit SHA + tag sémantique; utiliser digests dans manifests de déploiement pour immutabilité.

Q26: Que sont les images distroless ? Pourquoi les utiliser ?
A26: Images sans shell ni package manager, réduisent taille et surface d'attaque; adaptées pour runtime minimal.

Q27: Comment diagnostiquer une erreur de permission sur bind mount ?
A27: Vérifier UID/GID sur host, vérifier `:z`/`:Z` pour SELinux, ajuster permissions ou exécuter non-root avec matching UID.

Q28: Comment protéger votre registry contre accès non autorisé ?
A28: Authentification (TLS + credentials), RBAC, scan images avant push, network restrictions (IP whitelisting), image signing and policy.

Q29: Expliquez rootless Docker et cas d'usage.
A29: Exécution de daemon et containers sans privilèges root (moins de risque de compromission host) ; utile en environnements partagés.

Q30: Comment gérer l'immutabilité et la réconciliation des images dans production ?
A30: Déployer images par digest, automatiser rollbacks, intégrer GitOps pour déclaratif, utiliser outils de contrôle de versions pour manifests.

---