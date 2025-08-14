# 🔨 AWS CodeBuild - Build Automation Service
## Continuous Integration et Build Pipelines sur AWS

---

## 🎯 **AWS CodeBuild Overview**

### **Qu'est-ce que CodeBuild ?**
**AWS CodeBuild** est un service de build entièrement managé qui :
- Compile le code source
- Exécute les tests unitaires et d'intégration
- Produit des artefacts prêts au déploiement
- Scale automatiquement selon la demande
- Pay-per-use (pas de serveurs à gérer)

### **Avantages vs Jenkins traditionnel**
```yaml
AWS CodeBuild:
  ✅ Entièrement managé (pas de serveurs)
  ✅ Scaling automatique
  ✅ Pay-per-use
  ✅ Intégration native AWS
  ✅ Environnements pré-configurés
  ✅ Sécurité par défaut

Jenkins On-Premises:
  ❌ Gestion infrastructure
  ❌ Scaling manuel
  ❌ Coûts fixes
  ❌ Configuration sécurité complexe
  ✅ Plus de plugins disponibles
  ✅ Flexibilité maximale
```

---

## 🏗️ **Architecture et Concepts CodeBuild**

### **Composants Principaux**

#### **Build Project**
```yaml
Build Project Components:
  - Source Configuration: Où est le code
  - Environment: Runtime environment
  - Buildspec: Instructions de build
  - Artifacts: Où stocker les outputs
  - Service Role: Permissions IAM
  - VPC Configuration: Réseau (optionnel)
```

#### **Build Environment Types**
```yaml
Compute Types:
  build.general1.small:   # 3 GB RAM, 2 vCPUs
    - Prix: $0.005/minute
    - Usage: Projets simples, tests unitaires
    
  build.general1.medium:  # 7 GB RAM, 4 vCPUs  
    - Prix: $0.01/minute
    - Usage: Applications moyennes
    
  build.general1.large:   # 15 GB RAM, 8 vCPUs
    - Prix: $0.02/minute
    - Usage: Applications complexes, builds longs
    
  build.general1.2xlarge: # 144 GB RAM, 72 vCPUs
    - Prix: $0.20/minute
    - Usage: Projets très volumineux
```

### **Supported Images et Runtimes**

#### **AWS Managed Images**
```yaml
Ubuntu Standard 5.0:
  - Ubuntu 20.04
  - Docker 20.10
  - Languages: Node.js, Python, Java, .NET, Go, PHP
  - Tools: AWS CLI, Docker, Git

Amazon Linux 2:
  - Amazon Linux 2
  - Optimisé pour workloads AWS
  - Support ARM et x86

Windows Server Core 2019:
  - .NET Framework
  - Visual Studio Build Tools
  - PowerShell
```

---

## 📝 **Buildspec - Instructions de Build**

### **Structure Buildspec.yml**

#### **Buildspec Complet**
```yaml
version: 0.2

# Variables d'environnement
env:
  variables:
    NODE_ENV: production
    REGION: us-east-1
  parameter-store:
    DATABASE_URL: /myapp/prod/database_url
    API_KEY: /myapp/prod/api_key
  secrets-manager:
    DB_PASSWORD: prod/myapp/database:password

# Phases de build
phases:
  install:
    runtime-versions:
      nodejs: 16
      python: 3.9
    commands:
      - echo "Installing dependencies..."
      - npm install -g @aws-cdk/cli
      - pip install awscli
      
  pre_build:
    commands:
      - echo "Pre-build phase started"
      - echo "Logging in to Amazon ECR..."
      - aws ecr get-login-password --region $REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com
      - echo "Running tests..."
      - npm test
      - npm run lint
      
  build:
    commands:
      - echo "Build phase started"
      - echo "Building the application..."
      - npm run build
      - echo "Building Docker image..."
      - docker build -t $IMAGE_REPO_NAME:$IMAGE_TAG .
      - docker tag $IMAGE_REPO_NAME:$IMAGE_TAG $AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/$IMAGE_REPO_NAME:$IMAGE_TAG
      
  post_build:
    commands:
      - echo "Post-build phase started"
      - echo "Pushing Docker image to ECR..."
      - docker push $AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/$IMAGE_REPO_NAME:$IMAGE_TAG
      - echo "Creating image definitions file..."
      - printf '[{"name":"web","imageUri":"%s"}]' $AWS_ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/$IMAGE_REPO_NAME:$IMAGE_TAG > imagedefinitions.json

# Artefacts à conserver
artifacts:
  files:
    - '**/*'
  secondary-artifacts:
    BuildArtifact:
      files:
        - imagedefinitions.json
        - appspec.yml
      name: BuildArtifact
      
# Cache pour optimiser les builds
cache:
  paths:
    - '/root/.npm/**/*'
    - 'node_modules/**/*'
    - '/root/.cache/pip/**/*'

# Rapports (code coverage, tests)
reports:
  jest_reports:
    files:
      - 'coverage/clover.xml'
    file-format: 'CLOVERXML'
  eslint_reports:
    files:
      - 'eslint-report.xml'
    file-format: 'JUNITXML'
```

### **Buildspec pour Différents Languages**

#### **Node.js avec Jest Testing**
```yaml
version: 0.2
phases:
  install:
    runtime-versions:
      nodejs: 16
    commands:
      - npm install
      
  pre_build:
    commands:
      - echo "Running unit tests..."
      - npm test -- --coverage --watchAll=false
      - echo "Running linting..."
      - npm run lint
      - echo "Running security audit..."
      - npm audit --audit-level moderate
      
  build:
    commands:
      - echo "Building production bundle..."
      - npm run build
      
artifacts:
  files:
    - 'build/**/*'
    - 'package.json'
  name: WebAppBuild

reports:
  jest_reports:
    files:
      - 'coverage/clover.xml'
    file-format: 'CLOVERXML'
```

#### **Python avec pytest et Docker**
```yaml
version: 0.2
phases:
  install:
    runtime-versions:
      python: 3.9
      docker: 20
    commands:
      - pip install --upgrade pip
      - pip install -r requirements.txt
      - pip install pytest pytest-cov flake8
      
  pre_build:
    commands:
      - echo "Running Python tests..."
      - python -m pytest tests/ --cov=src --cov-report=xml
      - echo "Running code quality checks..."
      - flake8 src/
      - echo "Security scan..."
      - bandit -r src/
      
  build:
    commands:
      - echo "Building Docker image..."
      - docker build -t python-app:latest .
      - docker tag python-app:latest $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com/python-app:latest
      
  post_build:
    commands:
      - echo "Pushing to ECR..."
      - aws ecr get-login-password | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com
      - docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com/python-app:latest

artifacts:
  files:
    - Dockerfile
    - requirements.txt
    - appspec.yml
    
reports:
  pytest_reports:
    files:
      - 'coverage.xml'
    file-format: 'COBERTURAXML'
```

#### **Java with Maven**
```yaml
version: 0.2
phases:
  install:
    runtime-versions:
      java: corretto11
    commands:
      - echo "Installing Maven..."
      
  pre_build:
    commands:
      - echo "Running unit tests..."
      - mvn test
      - echo "Running static analysis..."
      - mvn sonar:sonar -Dsonar.host.url=$SONAR_URL -Dsonar.login=$SONAR_TOKEN
      
  build:
    commands:
      - echo "Building JAR file..."
      - mvn clean package -DskipTests
      - echo "Building Docker image..."
      - docker build -t java-app .
      
artifacts:
  files:
    - target/*.jar
    - Dockerfile
    - appspec.yml
  name: JavaAppBuild
```

---

## 🔧 **Configuration de Build Project**

### **Via CloudFormation**

#### **Build Project Complet**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'CodeBuild Project for DevOps Pipeline'

Parameters:
  RepositoryName:
    Type: String
    Default: my-devops-app
  
  BuildEnvironment:
    Type: String
    Default: build.general1.small
    AllowedValues:
      - build.general1.small
      - build.general1.medium
      - build.general1.large

Resources:
  # Service Role pour CodeBuild
  CodeBuildServiceRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: codebuild.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: CodeBuildServicePolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - logs:CreateLogGroup
                  - logs:CreateLogStream
                  - logs:PutLogEvents
                Resource: !Sub 'arn:aws:logs:${AWS::Region}:${AWS::AccountId}:log-group:/aws/codebuild/*'
              - Effect: Allow
                Action:
                  - codecommit:GitPull
                Resource: !Sub 'arn:aws:codecommit:${AWS::Region}:${AWS::AccountId}:${RepositoryName}'
              - Effect: Allow
                Action:
                  - s3:GetObject
                  - s3:PutObject
                Resource: !Sub '${ArtifactBucket}/*'
              - Effect: Allow
                Action:
                  - ecr:BatchCheckLayerAvailability
                  - ecr:GetDownloadUrlForLayer
                  - ecr:BatchGetImage
                  - ecr:GetAuthorizationToken
                Resource: '*'

  # S3 Bucket pour artefacts
  ArtifactBucket:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Sub '${AWS::StackName}-build-artifacts'
      VersioningConfiguration:
        Status: Enabled
      PublicAccessBlockConfiguration:
        BlockPublicAcls: true
        BlockPublicPolicy: true
        IgnorePublicAcls: true
        RestrictPublicBuckets: true

  # Build Project Principal
  CodeBuildProject:
    Type: AWS::CodeBuild::Project
    Properties:
      Name: !Sub '${AWS::StackName}-build'
      Description: 'Build project for DevOps pipeline'
      ServiceRole: !GetAtt CodeBuildServiceRole.Arn
      Artifacts:
        Type: S3
        Location: !Sub '${ArtifactBucket}/artifacts'
        Packaging: ZIP
      Environment:
        Type: LINUX_CONTAINER
        ComputeType: !Ref BuildEnvironment
        Image: aws/codebuild/standard:5.0
        PrivilegedMode: true  # Pour Docker builds
        EnvironmentVariables:
          - Name: AWS_DEFAULT_REGION
            Value: !Ref AWS::Region
          - Name: AWS_ACCOUNT_ID
            Value: !Ref AWS::AccountId
          - Name: IMAGE_REPO_NAME
            Value: my-app
          - Name: IMAGE_TAG
            Value: latest
      Source:
        Type: CODECOMMIT
        Location: !Sub 'https://git-codecommit.${AWS::Region}.amazonaws.com/v1/repos/${RepositoryName}'
        BuildSpec: |
          version: 0.2
          phases:
            install:
              runtime-versions:
                nodejs: 16
            pre_build:
              commands:
                - npm install
                - npm test
            build:
              commands:
                - npm run build
          artifacts:
            files:
              - '**/*'
      TimeoutInMinutes: 15
      Tags:
        - Key: Environment
          Value: Production
        - Key: Project
          Value: DevOps

  # CloudWatch Log Group
  CodeBuildLogGroup:
    Type: AWS::Logs::LogGroup
    Properties:
      LogGroupName: !Sub '/aws/codebuild/${AWS::StackName}-build'
      RetentionInDays: 14

Outputs:
  BuildProjectName:
    Description: 'Name of the CodeBuild project'
    Value: !Ref CodeBuildProject
    Export:
      Name: !Sub '${AWS::StackName}-build-project'
```

### **Multi-Environment Build Projects**

#### **Build Matrix Strategy**
```yaml
# Builds parallèles pour différents environnements
DevBuildProject:
  Type: AWS::CodeBuild::Project
  Properties:
    Name: app-build-dev
    Environment:
      EnvironmentVariables:
        - Name: ENVIRONMENT
          Value: development
        - Name: NODE_ENV
          Value: development

StagingBuildProject:
  Type: AWS::CodeBuild::Project
  Properties:
    Name: app-build-staging
    Environment:
      EnvironmentVariables:
        - Name: ENVIRONMENT
          Value: staging
        - Name: NODE_ENV
          Value: production

ProductionBuildProject:
  Type: AWS::CodeBuild::Project
  Properties:
    Name: app-build-prod
    Environment:
      EnvironmentVariables:
        - Name: ENVIRONMENT
          Value: production
        - Name: NODE_ENV
          Value: production
```

---

## 🐳 **Docker Builds avec CodeBuild**

### **Multi-Stage Docker Build**

#### **Dockerfile Optimisé**
```dockerfile
# Build stage
FROM node:16-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production && npm cache clean --force
COPY . .
RUN npm run build

# Production stage
FROM node:16-alpine AS production
WORKDIR /app
RUN addgroup -g 1001 -S nodejs && adduser -S nodejs -u 1001
COPY --from=builder --chown=nodejs:nodejs /app/dist ./dist
COPY --from=builder --chown=nodejs:nodejs /app/node_modules ./node_modules
COPY --from=builder --chown=nodejs:nodejs /app/package.json ./package.json

USER nodejs
EXPOSE 3000
CMD ["node", "dist/index.js"]
```

#### **Buildspec pour Multi-Architecture**
```yaml
version: 0.2
phases:
  install:
    runtime-versions:
      docker: 20
    commands:
      - echo "Setting up Docker buildx for multi-architecture builds"
      - docker run --rm --privileged multiarch/qemu-user-static --reset -p yes
      - docker buildx create --name mybuilder --use
      
  pre_build:
    commands:
      - echo "Logging in to Amazon ECR..."
      - aws ecr get-login-password --region $AWS_DEFAULT_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com
      
  build:
    commands:
      - echo "Building multi-architecture Docker images..."
      - |
        docker buildx build \
          --platform linux/amd64,linux/arm64 \
          --push \
          --tag $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com/my-app:latest \
          --tag $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com/my-app:$CODEBUILD_RESOLVED_SOURCE_VERSION \
          .
```

### **Security Scanning**

#### **Buildspec avec Security Tools**
```yaml
version: 0.2
phases:
  install:
    runtime-versions:
      docker: 20
    commands:
      - echo "Installing security tools..."
      - curl -sfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh | sh -s -- -b /usr/local/bin
      - pip install bandit safety
      
  pre_build:
    commands:
      - echo "Running security scans..."
      - echo "Scanning Python dependencies..."
      - safety check -r requirements.txt
      - echo "Scanning Python code..."
      - bandit -r src/ -f json -o bandit-report.json
      
  build:
    commands:
      - echo "Building Docker image..."
      - docker build -t security-app:latest .
      - echo "Scanning Docker image for vulnerabilities..."
      - trivy image --format json --output trivy-report.json security-app:latest
      - echo "Checking for high/critical vulnerabilities..."
      - trivy image --exit-code 1 --severity HIGH,CRITICAL security-app:latest
      
artifacts:
  files:
    - bandit-report.json
    - trivy-report.json
    - Dockerfile
    
reports:
  security_reports:
    files:
      - bandit-report.json
    file-format: 'RAW'
```

---

## 📊 **Monitoring et Optimization**

### **CloudWatch Metrics et Alarms**

#### **Key Metrics CodeBuild**
```yaml
Build Metrics:
  - Builds: Nombre de builds
  - Duration: Durée des builds
  - SucceededBuilds: Builds réussis
  - FailedBuilds: Builds échoués
  - DownloadSourceDuration: Temps téléchargement source
  - SubmittedBuilds: Builds en attente
```

#### **CloudWatch Alarms**
```yaml
BuildFailureAlarm:
  Type: AWS::CloudWatch::Alarm
  Properties:
    AlarmName: !Sub '${CodeBuildProject}-failures'
    AlarmDescription: 'Alert on CodeBuild failures'
    MetricName: FailedBuilds
    Namespace: AWS/CodeBuild
    Statistic: Sum
    Period: 300
    EvaluationPeriods: 1
    Threshold: 1
    ComparisonOperator: GreaterThanOrEqualToThreshold
    Dimensions:
      - Name: ProjectName
        Value: !Ref CodeBuildProject
    AlarmActions:
      - !Ref SNSTopicArn

LongBuildAlarm:
  Type: AWS::CloudWatch::Alarm
  Properties:
    AlarmName: !Sub '${CodeBuildProject}-long-duration'
    AlarmDescription: 'Alert on long build times'
    MetricName: Duration
    Namespace: AWS/CodeBuild
    Statistic: Average
    Period: 300
    EvaluationPeriods: 2
    Threshold: 900  # 15 minutes
    ComparisonOperator: GreaterThanThreshold
    Dimensions:
      - Name: ProjectName
        Value: !Ref CodeBuildProject
```

### **Build Optimization**

#### **Cache Strategy**
```yaml
# Dans buildspec.yml
cache:
  paths:
    # Node.js
    - '/root/.npm/**/*'
    - 'node_modules/**/*'
    
    # Python
    - '/root/.cache/pip/**/*'
    - '.venv/**/*'
    
    # Java Maven
    - '/root/.m2/**/*'
    
    # Docker layers
    - '/var/lib/docker/**/*'
```

#### **Parallel Builds Strategy**
```yaml
# Buildspec pour builds parallèles
version: 0.2
batch:
  fast-fail: true
  build-list:
    - identifier: unit_tests
      env:
        variables:
          TEST_TYPE: unit
      buildspec: |
        version: 0.2
        phases:
          install:
            runtime-versions:
              nodejs: 16
          build:
            commands:
              - npm test
              
    - identifier: integration_tests
      env:
        variables:
          TEST_TYPE: integration
      buildspec: |
        version: 0.2
        phases:
          install:
            runtime-versions:
              nodejs: 16
          build:
            commands:
              - npm run test:integration
              
    - identifier: security_scan
      env:
        variables:
          SCAN_TYPE: security
      buildspec: |
        version: 0.2
        phases:
          install:
            commands:
              - npm install -g audit-ci
          build:
            commands:
              - audit-ci --moderate
```

---

## ✅ **Quiz AWS CodeBuild**

### **Question 1:** Quelle est la différence entre les artefacts primaires et secondaires dans CodeBuild ?
<details>
<summary>Réponse</summary>

**Primary Artifacts :**
- Un seul artefact principal par build
- Défini dans la section `artifacts` du buildspec
- Utilisé par défaut par CodePipeline

**Secondary Artifacts :**
- Multiples artefacts nommés
- Définis dans `secondary-artifacts`
- Permet de séparer différents types d'outputs

**Exemple :**
```yaml
artifacts:
  files:
    - '**/*'  # Artefact principal
    
secondary-artifacts:
  TestResults:
    files:
      - 'test-results/**/*'
  DockerImage:
    files:
      - 'imagedefinitions.json'
```

**Cas d'usage :** Séparer les artefacts de déploiement des rapports de tests.
</details>

### **Question 2:** Comment optimiser les coûts de build CodeBuild ?
<details>
<summary>Réponse</summary>

**Stratégies d'optimisation :**

1. **Right-sizing compute types**
   - build.general1.small pour tests simples
   - build.general1.large seulement si nécessaire

2. **Build caching**
   - Cache dependencies (node_modules, .m2, pip cache)
   - Réutilise layers Docker

3. **Parallel builds**
   - Tests unitaires vs intégration en parallèle
   - Multi-architecture builds simultanés

4. **Build optimization**
   - Multi-stage Docker builds
   - Éliminer steps inutiles
   - Fast-fail sur erreurs

**Exemple cache :**
```yaml
cache:
  paths:
    - 'node_modules/**/*'
    - '/root/.npm/**/*'
```

**Économies :** 30-70% de réduction du temps de build.
</details>

### **Question 3:** Comment intégrer CodeBuild avec un VPC privé ?
<details>
<summary>Réponse</summary>

**Configuration VPC pour CodeBuild :**

```yaml
CodeBuildProject:
  Type: AWS::CodeBuild::Project
  Properties:
    VpcConfig:
      VpcId: !Ref VPC
      Subnets:
        - !Ref PrivateSubnet1
        - !Ref PrivateSubnet2
      SecurityGroupIds:
        - !Ref CodeBuildSecurityGroup

CodeBuildSecurityGroup:
  Type: AWS::EC2::SecurityGroup
  Properties:
    GroupDescription: Security group for CodeBuild
    VpcId: !Ref VPC
    SecurityGroupEgress:
      - IpProtocol: -1
        CidrIp: 0.0.0.0/0  # Internet access for dependencies
```

**Use Cases :**
- Accès à RDS privé pour tests d'intégration
- Pull dependencies depuis repositories privés
- Accès à services internes

**Requirements :**
- NAT Gateway ou VPC Endpoints pour Internet access
- Appropriate security groups
- Subnet avec suffisamment d'IPs disponibles
</details>

### **Question 4:** Comment implémenter des build notifications avec CodeBuild ?
<details>
<summary>Réponse</summary>

**CloudWatch Events + SNS :**

```yaml
BuildStateChangeRule:
  Type: AWS::Events::Rule
  Properties:
    EventPattern:
      source:
        - aws.codebuild
      detail-type:
        - CodeBuild Build State Change
      detail:
        build-status:
          - FAILED
          - SUCCEEDED
        project-name:
          - !Ref CodeBuildProject
    Targets:
      - Arn: !Ref SNSTopic
        Id: "BuildNotificationTarget"

# Lambda pour notifications Slack
BuildNotificationLambda:
  Type: AWS::Lambda::Function
  Properties:
    Runtime: python3.9
    Handler: index.lambda_handler
    Code:
      ZipFile: |
        import json
        import urllib3
        
        def lambda_handler(event, context):
            detail = event['detail']
            status = detail['build-status']
            project = detail['project-name']
            
            # Send to Slack webhook
            webhook_url = 'YOUR_SLACK_WEBHOOK'
            message = f"Build {status}: {project}"
            
            http = urllib3.PoolManager()
            response = http.request('POST', webhook_url,
                                  body=json.dumps({'text': message}),
                                  headers={'Content-Type': 'application/json'})
```
</details>

---

## 🎯 **Points Clés pour Certification AWS**

### **CodeBuild Core Concepts**
- **Fully managed** build service
- **Pay-per-use** pricing model
- **Automatic scaling** based on demand
- **Multiple compute types** for different workloads
- **Integration** with AWS services (S3, ECR, Parameter Store)

### **Best Practices**
- Use **caching** to reduce build times and costs
- Implement **parallel builds** for faster feedback
- Use **VPC configuration** for private resource access
- Configure **proper IAM roles** with least privilege
- Implement **build monitoring** with CloudWatch
- Use **buildspec.yml** in source code for version control

### **Security Considerations**
- **Never hardcode secrets** in buildspec
- Use **Parameter Store** or **Secrets Manager**
- Enable **VPC** for private resource access
- Implement **security scanning** in build process
- Use **signed commits** for source verification

---

**🎯 Next: AWS CodeDeploy - Deployment Automation →**
