# 🐳 Amazon ECR/ECS - Container Registry et Orchestration
## Container Management et Déploiement à l'Échelle

---

## 🎯 **Amazon ECR (Elastic Container Registry)**

### **Qu'est-ce qu'Amazon ECR ?**
**Amazon ECR** est un service de registry Docker entièrement managé qui :
- **Stocke et gère** les images de conteneurs Docker
- **S'intègre nativement** avec ECS, EKS, Fargate et Lambda
- **Fournit la sécurité** avec IAM, encryption et vulnerability scanning
- **Supporte** les registries publics et privés
- **Optimise** la distribution d'images avec CloudFront

### **ECR vs Docker Hub vs Autres Registries**
```yaml
Amazon ECR:
  ✅ Intégration native AWS
  ✅ IAM authentication intégrée
  ✅ Vulnerability scanning automatique
  ✅ Encryption at rest/in transit
  ✅ Lifecycle policies automatiques
  ✅ Multi-region replication
  ❌ Coût par GB stocké

Docker Hub:
  ✅ Large écosystème public
  ✅ Images officielles
  ✅ Free tier pour repos publics
  ❌ Rate limiting
  ❌ Moins d'intégration AWS
  ❌ Security features limitées

Harbor/Quay:
  ✅ Features enterprise (RBAC, audit)
  ✅ Multi-cloud compatibility
  ✅ Helm chart support
  ❌ Infrastructure à gérer
  ❌ Setup complexity plus élevée
```

### **ECR Repository Management**
```bash
# Créer un repository ECR
aws ecr create-repository \
    --repository-name my-app \
    --image-scanning-configuration scanOnPush=true \
    --encryption-configuration encryptionType=AES256

# Configuration de lifecycle policy
aws ecr put-lifecycle-policy \
    --repository-name my-app \
    --lifecycle-policy-text '{
        "rules": [
            {
                "rulePriority": 1,
                "description": "Keep last 10 production images",
                "selection": {
                    "tagStatus": "tagged",
                    "tagPrefixList": ["prod"],
                    "countType": "imageCountMoreThan",
                    "countNumber": 10
                },
                "action": {
                    "type": "expire"
                }
            },
            {
                "rulePriority": 2,
                "description": "Delete untagged images older than 1 day",
                "selection": {
                    "tagStatus": "untagged",
                    "countType": "sinceImagePushed",
                    "countUnit": "days",
                    "countNumber": 1
                },
                "action": {
                    "type": "expire"
                }
            }
        ]
    }'

# Push d'une image vers ECR
aws ecr get-login-password --region us-east-1 | \
    docker login --username AWS --password-stdin 123456789012.dkr.ecr.us-east-1.amazonaws.com

docker build -t my-app .
docker tag my-app:latest 123456789012.dkr.ecr.us-east-1.amazonaws.com/my-app:latest
docker push 123456789012.dkr.ecr.us-east-1.amazonaws.com/my-app:latest
```

---

## 🎯 **Amazon ECS (Elastic Container Service)**

### **Qu'est-ce qu'Amazon ECS ?**
**Amazon ECS** est un service d'orchestration de conteneurs qui :
- **Exécute et gère** les conteneurs Docker à l'échelle
- **Supporte deux modes** : EC2 et Fargate
- **S'intègre** avec ALB, Service Discovery, CloudWatch
- **Fournit** des patterns de déploiement avancés (Blue/Green, Rolling)
- **Gère** la haute disponibilité et scaling automatique

### **ECS vs EKS vs Fargate**
```yaml
Amazon ECS:
  ✅ Plus simple à gérer
  ✅ Intégration AWS native
  ✅ Coût inférieur pour workloads simples
  ✅ Courbe apprentissage plus faible
  ❌ Moins de flexibilité
  ❌ Écosystème plus petit

Amazon EKS:
  ✅ Kubernetes standard
  ✅ Écosystème riche (Helm, operators)
  ✅ Multi-cloud portability
  ✅ Communauté large
  ❌ Complexité plus élevée
  ❌ Coût de control plane

AWS Fargate:
  ✅ Serverless containers
  ✅ Pas d'infrastructure à gérer
  ✅ Pay-per-use granular
  ✅ Scaling automatique
  ❌ Moins de contrôle
  ❌ Coût plus élevé pour workloads constants
```

### **ECS Core Concepts**
```yaml
ECS Architecture:
  Cluster:
    - Groupe logique de resources compute
    - EC2 instances ou Fargate capacity
    - Namespace pour services et tasks
  
  Task Definition:
    - Blueprint pour containers
    - CPU/Memory requirements
    - Network configuration
    - IAM roles et security
  
  Service:
    - Maintient desired count of tasks
    - Load balancer integration
    - Service discovery
    - Deployment configuration
  
  Task:
    - Instance running d'une task definition
    - Un ou plusieurs containers
    - Placement sur cluster capacity
```

---

## 🏗️ **ECS Infrastructure avec CloudFormation**

### **ECS Cluster avec EC2 Launch Type**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'ECS Cluster with EC2 Launch Type'

Parameters:
  VpcId:
    Type: AWS::EC2::VPC::Id
    
  PrivateSubnetIds:
    Type: List<AWS::EC2::Subnet::Id>
    
  ClusterName:
    Type: String
    Default: my-ecs-cluster
    
  InstanceType:
    Type: String
    Default: t3.medium
    AllowedValues: [t3.small, t3.medium, t3.large, t3.xlarge]

Mappings:
  AWSRegionToAMI:
    us-east-1:
      AMIID: ami-0c6b1d09930fac512  # ECS Optimized AMI
    us-west-2:
      AMIID: ami-0cb2d76e7d6b6b0c9
    eu-west-1:
      AMIID: ami-0c4cd8e3cc93e7c7e

Resources:
  # ECS Cluster
  ECSCluster:
    Type: AWS::ECS::Cluster
    Properties:
      ClusterName: !Ref ClusterName
      ClusterSettings:
        - Name: containerInsights
          Value: enabled
      CapacityProviders:
        - EC2
        - FARGATE
        - FARGATE_SPOT
      DefaultCapacityProviderStrategy:
        - CapacityProvider: EC2
          Weight: 1
          Base: 0

  # Auto Scaling Group pour ECS
  ECSAutoScalingGroup:
    Type: AWS::AutoScaling::AutoScalingGroup
    Properties:
      VPCZoneIdentifier: !Ref PrivateSubnetIds
      LaunchTemplate:
        LaunchTemplateId: !Ref ECSLaunchTemplate
        Version: !GetAtt ECSLaunchTemplate.LatestVersionNumber
      MinSize: 1
      MaxSize: 10
      DesiredCapacity: 3
      TargetGroupARNs:
        - !Ref ECSTargetGroup
      HealthCheckType: ELB
      HealthCheckGracePeriod: 300
      Tags:
        - Key: Name
          Value: !Sub '${ClusterName}-ecs-instance'
          PropagateAtLaunch: true

  # Launch Template pour instances ECS
  ECSLaunchTemplate:
    Type: AWS::EC2::LaunchTemplate
    Properties:
      LaunchTemplateName: !Sub '${ClusterName}-launch-template'
      LaunchTemplateData:
        ImageId: !FindInMap [AWSRegionToAMI, !Ref 'AWS::Region', AMIID]
        InstanceType: !Ref InstanceType
        SecurityGroupIds:
          - !Ref ECSSecurityGroup
        IamInstanceProfile:
          Arn: !GetAtt ECSInstanceProfile.Arn
        UserData:
          Fn::Base64: !Sub |
            #!/bin/bash
            echo ECS_CLUSTER=${ECSCluster} >> /etc/ecs/ecs.config
            echo ECS_ENABLE_CONTAINER_METADATA=true >> /etc/ecs/ecs.config
            echo ECS_ENABLE_TASK_IAM_ROLE=true >> /etc/ecs/ecs.config
            echo ECS_ENABLE_TASK_IAM_ROLE_NETWORK_HOST=true >> /etc/ecs/ecs.config
            
            # Install CloudWatch agent
            yum install -y amazon-cloudwatch-agent
            
            # Start ECS agent
            systemctl enable ecs
            systemctl start ecs

  # IAM Role pour ECS instances
  ECSInstanceRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: ec2.amazonaws.com
            Action: sts:AssumeRole
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/service-role/AmazonEC2ContainerServiceforEC2Role
        - arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy
        - arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore
      Policies:
        - PolicyName: ECSInstancePolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - ecr:GetAuthorizationToken
                  - ecr:BatchCheckLayerAvailability
                  - ecr:GetDownloadUrlForLayer
                  - ecr:BatchGetImage
                Resource: '*'

  ECSInstanceProfile:
    Type: AWS::IAM::InstanceProfile
    Properties:
      Roles:
        - !Ref ECSInstanceRole

  # Security Group pour ECS instances
  ECSSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Security group for ECS instances
      VpcId: !Ref VpcId
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 32768
          ToPort: 65535
          SourceSecurityGroupId: !Ref ALBSecurityGroup
      Tags:
        - Key: Name
          Value: !Sub '${ClusterName}-ecs-sg'

  # Application Load Balancer
  ApplicationLoadBalancer:
    Type: AWS::ElasticLoadBalancingV2::LoadBalancer
    Properties:
      Name: !Sub '${ClusterName}-alb'
      Scheme: internet-facing
      Type: application
      Subnets: !Ref PublicSubnetIds  # Parameter à ajouter
      SecurityGroups:
        - !Ref ALBSecurityGroup

  ALBSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Security group for ALB
      VpcId: !Ref VpcId
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 80
          ToPort: 80
          CidrIp: 0.0.0.0/0
        - IpProtocol: tcp
          FromPort: 443
          ToPort: 443
          CidrIp: 0.0.0.0/0

  # Target Group pour ECS services
  ECSTargetGroup:
    Type: AWS::ElasticLoadBalancingV2::TargetGroup
    Properties:
      Name: !Sub '${ClusterName}-tg'
      Port: 80
      Protocol: HTTP
      VpcId: !Ref VpcId
      TargetType: instance
      HealthCheckPath: '/health'
      HealthCheckProtocol: HTTP
      HealthCheckIntervalSeconds: 30
      HealthCheckTimeoutSeconds: 5
      HealthyThresholdCount: 2
      UnhealthyThresholdCount: 3

  # ALB Listener
  ALBListener:
    Type: AWS::ElasticLoadBalancingV2::Listener
    Properties:
      DefaultActions:
        - Type: forward
          TargetGroupArn: !Ref ECSTargetGroup
      LoadBalancerArn: !Ref ApplicationLoadBalancer
      Port: 80
      Protocol: HTTP

  # Capacity Provider pour Auto Scaling
  ECSCapacityProvider:
    Type: AWS::ECS::CapacityProvider
    Properties:
      Name: !Sub '${ClusterName}-capacity-provider'
      AutoScalingGroupProvider:
        AutoScalingGroupArn: !Ref ECSAutoScalingGroup
        ManagedScaling:
          Status: ENABLED
          TargetCapacity: 80
          MinimumScalingStepSize: 1
          MaximumScalingStepSize: 10
        ManagedTerminationProtection: ENABLED

Outputs:
  ClusterName:
    Description: ECS Cluster Name
    Value: !Ref ECSCluster
    Export:
      Name: !Sub '${AWS::StackName}-cluster-name'
      
  LoadBalancerDNS:
    Description: Load Balancer DNS Name
    Value: !GetAtt ApplicationLoadBalancer.DNSName
    Export:
      Name: !Sub '${AWS::StackName}-alb-dns'
      
  TargetGroupArn:
    Description: Target Group ARN
    Value: !Ref ECSTargetGroup
    Export:
      Name: !Sub '${AWS::StackName}-target-group'
```

### **ECS Service avec Fargate**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'ECS Service running on Fargate'

Parameters:
  ClusterName:
    Type: String
    
  ServiceName:
    Type: String
    Default: my-web-service
    
  ImageURI:
    Type: String
    Description: ECR image URI
    
  ContainerPort:
    Type: Number
    Default: 80
    
  DesiredCount:
    Type: Number
    Default: 2

Resources:
  # Task Definition
  TaskDefinition:
    Type: AWS::ECS::TaskDefinition
    Properties:
      Family: !Sub '${ServiceName}-task'
      NetworkMode: awsvpc
      RequiresCompatibilities:
        - FARGATE
      Cpu: 256
      Memory: 512
      ExecutionRoleArn: !GetAtt TaskExecutionRole.Arn
      TaskRoleArn: !GetAtt TaskRole.Arn
      ContainerDefinitions:
        - Name: !Ref ServiceName
          Image: !Ref ImageURI
          PortMappings:
            - ContainerPort: !Ref ContainerPort
              Protocol: tcp
          Environment:
            - Name: NODE_ENV
              Value: production
            - Name: PORT
              Value: !Ref ContainerPort
          Secrets:
            - Name: DATABASE_URL
              ValueFrom: !Ref DatabaseSecret
          LogConfiguration:
            LogDriver: awslogs
            Options:
              awslogs-group: !Ref LogGroup
              awslogs-region: !Ref 'AWS::Region'
              awslogs-stream-prefix: ecs
          HealthCheck:
            Command:
              - CMD-SHELL
              - !Sub 'curl -f http://localhost:${ContainerPort}/health || exit 1'
            Interval: 30
            Timeout: 5
            Retries: 3
            StartPeriod: 60

  # ECS Service
  ECSService:
    Type: AWS::ECS::Service
    DependsOn: ALBListener
    Properties:
      ServiceName: !Ref ServiceName
      Cluster: !Ref ClusterName
      TaskDefinition: !Ref TaskDefinition
      LaunchType: FARGATE
      DesiredCount: !Ref DesiredCount
      NetworkConfiguration:
        AwsvpcConfiguration:
          SecurityGroups:
            - !Ref ServiceSecurityGroup
          Subnets: !Ref PrivateSubnetIds
          AssignPublicIp: DISABLED
      LoadBalancers:
        - ContainerName: !Ref ServiceName
          ContainerPort: !Ref ContainerPort
          TargetGroupArn: !Ref TargetGroup
      DeploymentConfiguration:
        DeploymentCircuitBreaker:
          Enable: true
          Rollback: true
        MaximumPercent: 200
        MinimumHealthyPercent: 50
      EnableExecuteCommand: true  # Pour debugging avec ECS Exec

  # Auto Scaling pour le service
  ServiceScalingTarget:
    Type: AWS::ApplicationAutoScaling::ScalableTarget
    Properties:
      MaxCapacity: 10
      MinCapacity: 2
      ResourceId: !Sub 'service/${ClusterName}/${ServiceName}'
      RoleARN: !GetAtt ApplicationAutoScalingRole.Arn
      ScalableDimension: ecs:service:DesiredCount
      ServiceNamespace: ecs

  ServiceScalingPolicy:
    Type: AWS::ApplicationAutoScaling::ScalingPolicy
    Properties:
      PolicyName: !Sub '${ServiceName}-scaling-policy'
      PolicyType: TargetTrackingScaling
      ScalingTargetId: !Ref ServiceScalingTarget
      TargetTrackingScalingPolicyConfiguration:
        PredefinedMetricSpecification:
          PredefinedMetricType: ECSServiceAverageCPUUtilization
        TargetValue: 70.0
        ScaleOutCooldown: 300
        ScaleInCooldown: 300

  # IAM Roles
  TaskExecutionRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: ecs-tasks.amazonaws.com
            Action: sts:AssumeRole
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
      Policies:
        - PolicyName: SecretsManagerAccess
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - secretsmanager:GetSecretValue
                Resource: !Ref DatabaseSecret

  TaskRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: ecs-tasks.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: TaskPermissions
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - s3:GetObject
                  - s3:PutObject
                Resource: !Sub '${S3Bucket}/*'

  # CloudWatch Log Group
  LogGroup:
    Type: AWS::Logs::LogGroup
    Properties:
      LogGroupName: !Sub '/ecs/${ServiceName}'
      RetentionInDays: 30

  # Secrets pour la base de données
  DatabaseSecret:
    Type: AWS::SecretsManager::Secret
    Properties:
      Description: Database credentials for ECS service
      GenerateSecretString:
        SecretStringTemplate: '{"username": "admin"}'
        GenerateStringKey: 'password'
        PasswordLength: 32
        ExcludeCharacters: '"@/\'

  # Security Group pour le service
  ServiceSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Security group for ECS service
      VpcId: !Ref VpcId
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: !Ref ContainerPort
          ToPort: !Ref ContainerPort
          SourceSecurityGroupId: !Ref ALBSecurityGroup

Outputs:
  ServiceName:
    Description: ECS Service Name
    Value: !Ref ECSService
    
  TaskDefinitionArn:
    Description: Task Definition ARN
    Value: !Ref TaskDefinition
```

---

## 🔄 **ECS Deployment Strategies**

### **Blue/Green Deployment avec CodeDeploy**
```yaml
# Blue/Green deployment configuration
BlueGreenDeployment:
  Type: AWS::CodeDeploy::DeploymentGroup
  Properties:
    ApplicationName: !Ref CodeDeployApplication
    DeploymentGroupName: !Sub '${ServiceName}-bg-deployment'
    ServiceRoleArn: !GetAtt CodeDeployServiceRole.Arn
    BlueGreenDeploymentConfiguration:
      TerminateBlueInstancesOnDeploymentSuccess:
        Action: TERMINATE
        TerminationWaitTimeInMinutes: 5
      DeploymentReadyOption:
        ActionOnTimeout: CONTINUE_DEPLOYMENT
        WaitTimeInMinutes: 0
      GreenFleetProvisioningOption:
        Action: COPY_AUTO_SCALING_GROUP
    LoadBalancerInfo:
      TargetGroupInfoList:
        - Name: !GetAtt BlueTargetGroup.TargetGroupName
        - Name: !GetAtt GreenTargetGroup.TargetGroupName
    AutoRollbackConfiguration:
      Enabled: true
      Events:
        - DEPLOYMENT_FAILURE
        - DEPLOYMENT_STOP_ON_ALARM
        - DEPLOYMENT_STOP_ON_INSTANCE_FAILURE

# Service Discovery pour microservices
ServiceDiscoveryNamespace:
  Type: AWS::ServiceDiscovery::PrivateDnsNamespace
  Properties:
    Name: local
    Vpc: !Ref VpcId

ServiceDiscoveryService:
  Type: AWS::ServiceDiscovery::Service
  Properties:
    Name: !Ref ServiceName
    DnsConfig:
      DnsRecords:
        - Type: A
          TTL: 300
      NamespaceId: !Ref ServiceDiscoveryNamespace
    HealthCheckCustomConfig:
      FailureThreshold: 1
```

### **Rolling Deployment avec Circuit Breaker**
```yaml
# Circuit breaker configuration pour rolling deployments
DeploymentConfiguration:
  DeploymentCircuitBreaker:
    Enable: true
    Rollback: true
  MaximumPercent: 200
  MinimumHealthyPercent: 50
  
# CloudWatch Alarms pour deployment health
DeploymentAlarm:
  Type: AWS::CloudWatch::Alarm
  Properties:
    AlarmName: !Sub '${ServiceName}-deployment-alarm'
    AlarmDescription: 'Monitor ECS service health during deployment'
    MetricName: HealthyHostCount
    Namespace: AWS/ApplicationELB
    Statistic: Average
    Period: 60
    EvaluationPeriods: 2
    Threshold: 1
    ComparisonOperator: LessThanThreshold
    Dimensions:
      - Name: TargetGroup
        Value: !GetAtt TargetGroup.TargetGroupFullName
```

---

## 🔗 **Intégrations ECS avec Services AWS**

### **ECS avec CodePipeline**
```yaml
# Pipeline intégrant ECR, ECS et CodeDeploy
ECSPipeline:
  Type: AWS::CodePipeline::Pipeline
  Properties:
    Stages:
      - Name: Source
        Actions:
          - Name: SourceAction
            ActionTypeId:
              Category: Source
              Owner: AWS
              Provider: CodeCommit
              Version: 1
            Configuration:
              RepositoryName: !Ref RepositoryName
              BranchName: main
            OutputArtifacts:
              - Name: SourceOutput

      - Name: Build
        Actions:
          - Name: Build
            ActionTypeId:
              Category: Build
              Owner: AWS
              Provider: CodeBuild
              Version: 1
            Configuration:
              ProjectName: !Ref BuildProject
            InputArtifacts:
              - Name: SourceOutput
            OutputArtifacts:
              - Name: BuildOutput

      - Name: Deploy
        Actions:
          - Name: CreateChangeSet
            ActionTypeId:
              Category: Deploy
              Owner: AWS
              Provider: CloudFormation
              Version: 1
            Configuration:
              ActionMode: CHANGE_SET_REPLACE
              StackName: !Sub '${ServiceName}-service'
              ChangeSetName: !Sub '${ServiceName}-changeset'
              TemplatePath: BuildOutput::service-template.yaml
              Capabilities: CAPABILITY_IAM
              RoleArn: !GetAtt CloudFormationRole.Arn
              ParameterOverrides: !Sub |
                {
                  "ImageURI": "${AWS::AccountId}.dkr.ecr.${AWS::Region}.amazonaws.com/${ECRRepository}:${ImageTag}"
                }
            InputArtifacts:
              - Name: BuildOutput
            RunOrder: 1

          - Name: ExecuteChangeSet
            ActionTypeId:
              Category: Deploy
              Owner: AWS
              Provider: CloudFormation
              Version: 1
            Configuration:
              ActionMode: CHANGE_SET_EXECUTE
              StackName: !Sub '${ServiceName}-service'
              ChangeSetName: !Sub '${ServiceName}-changeset'
            RunOrder: 2

# BuildSpec pour construire et pusher vers ECR
BuildProject:
  Type: AWS::CodeBuild::Project
  Properties:
    ServiceRole: !GetAtt CodeBuildRole.Arn
    Artifacts:
      Type: CODEPIPELINE
    Environment:
      Type: LINUX_CONTAINER
      ComputeType: BUILD_GENERAL1_SMALL
      Image: aws/codebuild/standard:5.0
      PrivilegedMode: true
      EnvironmentVariables:
        - Name: AWS_DEFAULT_REGION
          Value: !Ref 'AWS::Region'
        - Name: AWS_ACCOUNT_ID
          Value: !Ref 'AWS::AccountId'
        - Name: IMAGE_REPO_NAME
          Value: !Ref ECRRepository
        - Name: IMAGE_TAG
          Value: latest
    Source:
      Type: CODEPIPELINE
      BuildSpec: |
        version: 0.2
        phases:
          pre_build:
            commands:
              - echo Logging in to Amazon ECR...
              - aws ecr get-login-password --region $AWS_DEFAULT_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com
          build:
            commands:
              - echo Build started on `date`
              - echo Building the Docker image...
              - docker build -t $IMAGE_REPO_NAME:$IMAGE_TAG .
              - docker tag $IMAGE_REPO_NAME:$IMAGE_TAG $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com/$IMAGE_REPO_NAME:$IMAGE_TAG
          post_build:
            commands:
              - echo Build completed on `date`
              - echo Pushing the Docker image...
              - docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com/$IMAGE_REPO_NAME:$IMAGE_TAG
              - echo Writing image definitions file...
              - printf '[{"name":"'$CONTAINER_NAME'","imageUri":"'$AWS_ACCOUNT_ID'.dkr.ecr.'$AWS_DEFAULT_REGION'.amazonaws.com/'$IMAGE_REPO_NAME':'$IMAGE_TAG'"}]' > imagedefinitions.json
        artifacts:
          files:
            - imagedefinitions.json
            - service-template.yaml
```

### **ECS avec X-Ray et CloudWatch**
```yaml
# Task definition avec X-Ray sidecar
TaskDefinitionWithXRay:
  Type: AWS::ECS::TaskDefinition
  Properties:
    ContainerDefinitions:
      # Application container
      - Name: app
        Image: !Ref ImageURI
        PortMappings:
          - ContainerPort: 80
        Environment:
          - Name: AWS_XRAY_TRACING_NAME
            Value: !Ref ServiceName
          - Name: AWS_XRAY_DAEMON_ADDRESS
            Value: localhost:2000
        DependsOn:
          - ContainerName: xray-daemon
            Condition: START
        
      # X-Ray daemon sidecar
      - Name: xray-daemon
        Image: amazon/aws-xray-daemon:latest
        PortMappings:
          - ContainerPort: 2000
            Protocol: udp
        Environment:
          - Name: AWS_REGION
            Value: !Ref 'AWS::Region'

# CloudWatch Container Insights
ClusterWithContainerInsights:
  Type: AWS::ECS::Cluster
  Properties:
    ClusterSettings:
      - Name: containerInsights
        Value: enabled

# Custom CloudWatch Dashboard
ECSMonitoringDashboard:
  Type: AWS::CloudWatch::Dashboard
  Properties:
    DashboardName: !Sub '${ServiceName}-monitoring'
    DashboardBody: !Sub |
      {
        "widgets": [
          {
            "type": "metric",
            "properties": {
              "metrics": [
                ["AWS/ECS", "CPUUtilization", "ServiceName", "${ServiceName}", "ClusterName", "${ClusterName}"],
                ["AWS/ECS", "MemoryUtilization", "ServiceName", "${ServiceName}", "ClusterName", "${ClusterName}"],
                ["AWS/ApplicationELB", "TargetResponseTime", "LoadBalancer", "${LoadBalancer}"],
                ["AWS/ApplicationELB", "RequestCount", "LoadBalancer", "${LoadBalancer}"]
              ],
              "period": 300,
              "stat": "Average",
              "region": "${AWS::Region}",
              "title": "ECS Service Metrics"
            }
          }
        ]
      }
```

---

## ✅ **Quiz ECR/ECS**

### **Question 1:** Quelles sont les différences entre les launch types EC2 et Fargate dans ECS ?
<details>
<summary>Réponse</summary>

**EC2 Launch Type :**
- **Infrastructure contrôlée** : Vous gérez les instances EC2
- **Coût optimisé** pour workloads constants
- **Accès complet** à l'instance (SSH, debugging)
- **Personnalisation** OS et runtime
- **Placement constraints** avancés
- **Ressources réservées** (CPU/Memory non utilisées payées)

**Fargate Launch Type :**
- **Serverless containers** : AWS gère l'infrastructure
- **Pay-per-use** granulaire (par vCPU/Memory)
- **Pas d'accès instance** underlying
- **Scaling automatique** et rapide
- **Simplified operations** et maintenance
- **Cold start** potentiel pour nouveaux tasks

**Quand utiliser quoi :**
- **EC2** : Workloads constants, besoins personnalisation, coût optimization
- **Fargate** : Workloads variables, microservices, rapid scaling, ops simplifiées

**Hybrid approach** possible avec capacity providers.
</details>

### **Question 2:** Comment optimiser les coûts avec ECR et ECS ?
<details>
<summary>Réponse</summary>

**ECR Cost Optimization :**

1. **Lifecycle Policies :**
```json
{
  "rules": [
    {
      "rulePriority": 1,
      "description": "Keep last 10 production images",
      "selection": {
        "tagStatus": "tagged",
        "tagPrefixList": ["prod"],
        "countType": "imageCountMoreThan",
        "countNumber": 10
      },
      "action": { "type": "expire" }
    }
  ]
}
```

2. **Image Optimization :**
- Multi-stage Docker builds
- Minimal base images (Alpine, distroless)
- Layer caching optimization
- Image compression

**ECS Cost Optimization :**

1. **Right-sizing :**
- Monitor CPU/Memory utilization
- Use Compute Optimizer recommendations
- Adjust task definition resources

2. **Launch Type Selection :**
- EC2 pour workloads constants
- Fargate Spot pour dev/test
- Mixed capacity providers

3. **Auto Scaling :**
- Target tracking scaling policies
- Scheduled scaling pour patterns prédictibles
- Scale-in protection pour stateful services

4. **Reserved Capacity :**
- Reserved Instances pour EC2 launch type
- Savings Plans pour Fargate
</details>

### **Question 3:** Quelles intégrations ECS avec autres services AWS sont importantes pour DevOps ?
<details>
<summary>Réponse</summary>

**Developer Tools Integration :**
- **CodeCommit** : Source code repositories
- **CodeBuild** : Container image building et testing
- **CodePipeline** : CI/CD orchestration
- **CodeDeploy** : Blue/Green deployments pour ECS

**Monitoring & Observability :**
- **CloudWatch** : Metrics, logs, alarms et Container Insights
- **X-Ray** : Distributed tracing avec sidecar containers
- **AWS Distro for OpenTelemetry** : Observability standard

**Networking & Service Discovery :**
- **Application Load Balancer** : Traffic distribution et health checks
- **AWS Cloud Map** : Service discovery et DNS
- **VPC** : Network isolation et security groups

**Security & Secrets :**
- **IAM** : Task roles et execution roles
- **Secrets Manager** : Database credentials et API keys
- **Parameter Store** : Configuration management
- **Certificate Manager** : SSL/TLS certificates

**Storage & Database :**
- **EFS** : Shared file systems entre containers
- **RDS** : Managed databases avec security groups
- **DynamoDB** : NoSQL databases avec IAM integration

**Event-Driven Architecture :**
- **EventBridge** : Application event routing
- **SNS/SQS** : Messaging patterns
- **Lambda** : Serverless functions triggered by ECS events
</details>

### **Question 4:** Comment implémenter la sécurité dans ECR et ECS ?
<details>
<summary>Réponse</summary>

**ECR Security :**

1. **Image Scanning :**
```bash
# Enable scan on push
aws ecr put-image-scanning-configuration \
    --repository-name my-repo \
    --image-scanning-configuration scanOnPush=true
```

2. **Access Control :**
- IAM policies pour push/pull permissions
- Cross-account access avec resource policies
- VPC endpoints pour private connectivity

3. **Encryption :**
- Encryption at rest avec KMS
- HTTPS pour data in transit
- Signed images avec Docker Content Trust

**ECS Security :**

1. **Network Security :**
- VPC avec private subnets
- Security groups restrictifs
- AWS PrivateLink pour service communication

2. **IAM Best Practices :**
```yaml
TaskRole:
  Type: AWS::IAM::Role
  Properties:
    AssumeRolePolicyDocument:
      Version: '2012-10-17'
      Statement:
        - Effect: Allow
          Principal:
            Service: ecs-tasks.amazonaws.com
          Action: sts:AssumeRole
    Policies:
      - PolicyName: MinimalPermissions
        PolicyDocument:
          Version: '2012-10-17'
          Statement:
            - Effect: Allow
              Action:
                - s3:GetObject
              Resource: !Sub 'arn:aws:s3:::my-bucket/app-data/*'
```

3. **Runtime Security :**
- Read-only file systems
- Non-root user containers
- Resource limits (CPU/Memory)
- Health checks et readiness probes

4. **Secrets Management :**
- Secrets Manager integration
- Environment variables encryption
- Parameter Store avec SecureString

5. **Compliance :**
- AWS Config rules pour ECS compliance
- CloudTrail logging pour audit
- GuardDuty pour threat detection
</details>

---

## 🎯 **Points Clés pour Certification AWS**

### **ECR Core Features**
- **Private et public registries** avec IAM integration
- **Vulnerability scanning** automatique et manual
- **Lifecycle policies** pour image cleanup
- **Cross-region replication** pour disaster recovery
- **Integration native** avec ECS, EKS, Lambda

### **ECS Architecture**
- **Launch types** : EC2 vs Fargate trade-offs
- **Service discovery** avec AWS Cloud Map
- **Load balancing** avec ALB/NLB integration
- **Auto scaling** : Service et cluster level
- **Deployment strategies** : Rolling, Blue/Green

### **DevOps Integration**
- **CI/CD pipelines** avec CodePipeline et CodeDeploy
- **Container monitoring** avec CloudWatch Container Insights
- **Distributed tracing** avec X-Ray sidecar pattern
- **Event-driven automation** avec EventBridge
- **Security best practices** avec IAM task roles

### **Cost Optimization**
- **Capacity providers** pour mixed workloads
- **Spot instances** pour Fargate et EC2
- **Resource right-sizing** avec monitoring
- **Image optimization** et lifecycle management

---

**🎯 Next: AWS Lambda - Serverless et Event-Driven Architecture →**
