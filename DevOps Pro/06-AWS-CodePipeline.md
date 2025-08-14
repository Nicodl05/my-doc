# 🔄 AWS CodePipeline - Orchestration CI/CD
## Pipelines End-to-End et Workflow Automation

---

## 🎯 **AWS CodePipeline Overview**

### **Qu'est-ce que CodePipeline ?**
**AWS CodePipeline** est un service d'intégration et déploiement continus qui :
- Orchestre l'ensemble du workflow CI/CD
- Connecte tous les services AWS Developer Tools
- S'intègre avec des outils tiers (GitHub, Jenkins, etc.)
- Fournit une visualisation du pipeline
- Automatise les releases avec des approbations manuelles

### **Avantages vs Solutions Traditionnelles**
```yaml
AWS CodePipeline:
  ✅ Entièrement managé
  ✅ Pay-per-pipeline-execution
  ✅ Intégration native AWS
  ✅ Visual workflow
  ✅ Parallel/Sequential stages
  ✅ Cross-account deployments

Jenkins/GitLab CI:
  ❌ Infrastructure à gérer
  ❌ Coûts fixes
  ❌ Configuration AWS complexe
  ✅ Plus de plugins
  ✅ Flexibilité script
  ✅ Open source
```

---

## 🏗️ **Architecture et Concepts CodePipeline**

### **Composants Principaux**

#### **Pipeline Structure**
```yaml
Pipeline:
  Stages:
    - Source Stage:
        Actions:
          - Source Action (CodeCommit, S3, GitHub)
    - Build Stage:
        Actions:
          - Build Action (CodeBuild, Jenkins)
    - Test Stage:
        Actions:
          - Test Action (CodeBuild, Third-party)
    - Deploy Stage:
        Actions:
          - Deploy Action (CodeDeploy, ECS, CloudFormation)

Execution Flow:
  - Séquentiel entre stages
  - Parallèle au sein d'un stage
  - Artifacts passés entre actions
  - Approval gates optionnels
```

#### **Action Types et Providers**
```yaml
Source Actions:
  - AWS CodeCommit
  - Amazon S3
  - GitHub (v1/v2)
  - Bitbucket
  - Amazon ECR

Build Actions:
  - AWS CodeBuild
  - Jenkins
  - TeamCity
  - SonarQube

Test Actions:
  - AWS CodeBuild
  - AWS Device Farm
  - Third-party testing tools

Deploy Actions:
  - AWS CodeDeploy
  - AWS CloudFormation
  - Amazon ECS
  - Amazon S3
  - AWS Elastic Beanstalk
  - AWS Lambda
  - AWS Service Catalog

Invoke Actions:
  - AWS Lambda
  - Amazon SNS
  - AWS Step Functions
```

---

## 🔧 **Configuration Pipeline CloudFormation**

### **Pipeline Complet Multi-Stage**

#### **Master Pipeline Template**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Complete CI/CD Pipeline with CodePipeline'

Parameters:
  RepositoryName:
    Type: String
    Default: my-web-app
  
  BranchName:
    Type: String
    Default: main
    
  EnvironmentName:
    Type: String
    Default: production

Resources:
  # S3 Bucket pour artifacts
  ArtifactStore:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Sub '${AWS::StackName}-pipeline-artifacts'
      VersioningConfiguration:
        Status: Enabled
      BucketEncryption:
        ServerSideEncryptionConfiguration:
          - ServerSideEncryptionByDefault:
              SSEAlgorithm: AES256
      PublicAccessBlockConfiguration:
        BlockPublicAcls: true
        BlockPublicPolicy: true
        IgnorePublicAcls: true
        RestrictPublicBuckets: true

  # KMS Key pour chiffrement
  ArtifactStoreKMSKey:
    Type: AWS::KMS::Key
    Properties:
      Description: 'KMS Key for CodePipeline artifacts'
      KeyPolicy:
        Statement:
          - Sid: Enable IAM User Permissions
            Effect: Allow
            Principal:
              AWS: !Sub 'arn:aws:iam::${AWS::AccountId}:root'
            Action: 'kms:*'
            Resource: '*'
          - Sid: Allow CodePipeline
            Effect: Allow
            Principal:
              Service:
                - codepipeline.amazonaws.com
                - codebuild.amazonaws.com
            Action:
              - kms:Decrypt
              - kms:DescribeKey
              - kms:Encrypt
              - kms:GenerateDataKey*
              - kms:ReEncrypt*
            Resource: '*'

  # Service Role pour CodePipeline
  CodePipelineServiceRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: codepipeline.amazonaws.com
            Action: sts:AssumeRole
      Policies:
        - PolicyName: CodePipelineServicePolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              # S3 Artifacts
              - Effect: Allow
                Action:
                  - s3:GetBucketVersioning
                  - s3:GetObject
                  - s3:GetObjectVersion
                  - s3:PutObject
                Resource:
                  - !Sub '${ArtifactStore}/*'
                  - !GetAtt ArtifactStore.Arn
              
              # CodeCommit
              - Effect: Allow
                Action:
                  - codecommit:CancelUploadArchive
                  - codecommit:GetBranch
                  - codecommit:GetCommit
                  - codecommit:GetRepository
                  - codecommit:ListBranches
                  - codecommit:ListRepositories
                Resource: !Sub 'arn:aws:codecommit:${AWS::Region}:${AWS::AccountId}:${RepositoryName}'
              
              # CodeBuild
              - Effect: Allow
                Action:
                  - codebuild:BatchGetBuilds
                  - codebuild:StartBuild
                Resource: 
                  - !GetAtt BuildProject.Arn
                  - !GetAtt TestProject.Arn
              
              # CodeDeploy
              - Effect: Allow
                Action:
                  - codedeploy:CreateDeployment
                  - codedeploy:GetApplication
                  - codedeploy:GetApplicationRevision
                  - codedeploy:GetDeployment
                  - codedeploy:GetDeploymentConfig
                  - codedeploy:RegisterApplicationRevision
                Resource: '*'
              
              # CloudFormation
              - Effect: Allow
                Action:
                  - cloudformation:CreateStack
                  - cloudformation:DeleteStack
                  - cloudformation:DescribeStacks
                  - cloudformation:UpdateStack
                  - cloudformation:CreateChangeSet
                  - cloudformation:DeleteChangeSet
                  - cloudformation:DescribeChangeSet
                  - cloudformation:ExecuteChangeSet
                  - cloudformation:SetStackPolicy
                  - cloudformation:ValidateTemplate
                Resource: '*'
              
              # KMS
              - Effect: Allow
                Action:
                  - kms:Decrypt
                  - kms:GenerateDataKey
                Resource: !GetAtt ArtifactStoreKMSKey.Arn

  # Build Project
  BuildProject:
    Type: AWS::CodeBuild::Project
    Properties:
      Name: !Sub '${AWS::StackName}-build'
      ServiceRole: !GetAtt CodeBuildServiceRole.Arn
      Artifacts:
        Type: CODEPIPELINE
      Environment:
        Type: LINUX_CONTAINER
        ComputeType: build.general1.small
        Image: aws/codebuild/standard:5.0
        PrivilegedMode: true
        EnvironmentVariables:
          - Name: AWS_DEFAULT_REGION
            Value: !Ref AWS::Region
          - Name: AWS_ACCOUNT_ID
            Value: !Ref AWS::AccountId
      Source:
        Type: CODEPIPELINE
        BuildSpec: |
          version: 0.2
          phases:
            install:
              runtime-versions:
                nodejs: 16
            pre_build:
              commands:
                - echo "Installing dependencies..."
                - npm install
            build:
              commands:
                - echo "Running build..."
                - npm run build
                - echo "Running unit tests..."
                - npm test
          artifacts:
            files:
              - '**/*'

  # Test Project (Integration Tests)
  TestProject:
    Type: AWS::CodeBuild::Project
    Properties:
      Name: !Sub '${AWS::StackName}-test'
      ServiceRole: !GetAtt CodeBuildServiceRole.Arn
      Artifacts:
        Type: CODEPIPELINE
      Environment:
        Type: LINUX_CONTAINER
        ComputeType: build.general1.small
        Image: aws/codebuild/standard:5.0
      Source:
        Type: CODEPIPELINE
        BuildSpec: |
          version: 0.2
          phases:
            install:
              runtime-versions:
                nodejs: 16
            pre_build:
              commands:
                - npm install
            build:
              commands:
                - echo "Running integration tests..."
                - npm run test:integration
                - echo "Running security tests..."
                - npm audit --audit-level moderate

  # CodeDeploy Application (from previous template)
  CodeDeployApplication:
    Type: AWS::CodeDeploy::Application
    Properties:
      ApplicationName: !Sub '${AWS::StackName}-app'
      ComputePlatform: Server

  # Main Pipeline
  CodePipeline:
    Type: AWS::CodePipeline::Pipeline
    Properties:
      Name: !Sub '${AWS::StackName}-pipeline'
      RoleArn: !GetAtt CodePipelineServiceRole.Arn
      ArtifactStore:
        Type: S3
        Location: !Ref ArtifactStore
        EncryptionKey:
          Id: !GetAtt ArtifactStoreKMSKey.Arn
          Type: KMS
      Stages:
        # Source Stage
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
                BranchName: !Ref BranchName
                PollForSourceChanges: false  # Use CloudWatch Events
              OutputArtifacts:
                - Name: SourceOutput
        
        # Build Stage
        - Name: Build
          Actions:
            - Name: BuildAction
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
        
        # Test Stage
        - Name: Test
          Actions:
            - Name: IntegrationTests
              ActionTypeId:
                Category: Test
                Owner: AWS
                Provider: CodeBuild
                Version: 1
              Configuration:
                ProjectName: !Ref TestProject
              InputArtifacts:
                - Name: BuildOutput
              RunOrder: 1
            
            - Name: SecurityScan
              ActionTypeId:
                Category: Invoke
                Owner: AWS
                Provider: Lambda
                Version: 1
              Configuration:
                FunctionName: !Ref SecurityScanFunction
              InputArtifacts:
                - Name: BuildOutput
              RunOrder: 2
        
        # Manual Approval
        - Name: Approval
          Actions:
            - Name: ManualApproval
              ActionTypeId:
                Category: Approval
                Owner: AWS
                Provider: Manual
                Version: 1
              Configuration:
                CustomData: 'Please review the build and test results before deploying to production'
                NotificationArn: !Ref ApprovalTopic
        
        # Deploy to Staging
        - Name: DeployStaging
          Actions:
            - Name: CreateStagingChangeSet
              ActionTypeId:
                Category: Deploy
                Owner: AWS
                Provider: CloudFormation
                Version: 1
              Configuration:
                ActionMode: CHANGE_SET_REPLACE
                StackName: !Sub '${AWS::StackName}-staging'
                ChangeSetName: !Sub '${AWS::StackName}-staging-changeset'
                TemplatePath: BuildOutput::staging-template.yaml
                Capabilities: CAPABILITY_IAM
                RoleArn: !GetAtt CloudFormationRole.Arn
              InputArtifacts:
                - Name: BuildOutput
              RunOrder: 1
            
            - Name: ExecuteStagingChangeSet
              ActionTypeId:
                Category: Deploy
                Owner: AWS
                Provider: CloudFormation
                Version: 1
              Configuration:
                ActionMode: CHANGE_SET_EXECUTE
                StackName: !Sub '${AWS::StackName}-staging'
                ChangeSetName: !Sub '${AWS::StackName}-staging-changeset'
              RunOrder: 2
        
        # Deploy to Production
        - Name: DeployProduction
          Actions:
            - Name: CreateProductionChangeSet
              ActionTypeId:
                Category: Deploy
                Owner: AWS
                Provider: CloudFormation
                Version: 1
              Configuration:
                ActionMode: CHANGE_SET_REPLACE
                StackName: !Sub '${AWS::StackName}-production'
                ChangeSetName: !Sub '${AWS::StackName}-production-changeset'
                TemplatePath: BuildOutput::production-template.yaml
                Capabilities: CAPABILITY_IAM
                RoleArn: !GetAtt CloudFormationRole.Arn
              InputArtifacts:
                - Name: BuildOutput
              RunOrder: 1
            
            - Name: ExecuteProductionChangeSet
              ActionTypeId:
                Category: Deploy
                Owner: AWS
                Provider: CloudFormation
                Version: 1
              Configuration:
                ActionMode: CHANGE_SET_EXECUTE
                StackName: !Sub '${AWS::StackName}-production'
                ChangeSetName: !Sub '${AWS::StackName}-production-changeset'
              RunOrder: 2

  # CloudWatch Event Rule pour déclencher le pipeline
  PipelineTriggerRule:
    Type: AWS::Events::Rule
    Properties:
      Description: 'Trigger pipeline on CodeCommit push'
      EventPattern:
        source:
          - aws.codecommit
        detail-type:
          - CodeCommit Repository State Change
        detail:
          event:
            - pushToRepository
          repositoryName:
            - !Ref RepositoryName
          referenceName:
            - !Sub 'refs/heads/${BranchName}'
      Targets:
        - Arn: !Sub 'arn:aws:codepipeline:${AWS::Region}:${AWS::AccountId}:pipeline/${CodePipeline}'
          Id: "CodePipelineTarget"
          RoleArn: !GetAtt EventsRole.Arn

  # SNS Topic pour approbations
  ApprovalTopic:
    Type: AWS::SNS::Topic
    Properties:
      TopicName: !Sub '${AWS::StackName}-approvals'
      DisplayName: 'Pipeline Approval Notifications'

Outputs:
  PipelineName:
    Description: 'Name of the CodePipeline'
    Value: !Ref CodePipeline
    Export:
      Name: !Sub '${AWS::StackName}-pipeline-name'
      
  PipelineUrl:
    Description: 'URL of the CodePipeline console'
    Value: !Sub 'https://console.aws.amazon.com/codesuite/codepipeline/pipelines/${CodePipeline}/view'
```

---

## 🌍 **Multi-Account et Cross-Region Pipelines**

### **Cross-Account Deployment Pattern**

#### **Pipeline Master Account Setup**
```yaml
# Master account - Pipeline orchestration
CrossAccountRole:
  Type: AWS::IAM::Role
  Properties:
    RoleName: CodePipelineCrossAccountRole
    AssumeRolePolicyDocument:
      Version: '2012-10-17'
      Statement:
        - Effect: Allow
          Principal:
            AWS: 
              - !Sub 'arn:aws:iam::${StagingAccountId}:root'
              - !Sub 'arn:aws:iam::${ProductionAccountId}:root'
          Action: sts:AssumeRole
    Policies:
      - PolicyName: CrossAccountDeployment
        PolicyDocument:
          Version: '2012-10-17'
          Statement:
            - Effect: Allow
              Action:
                - cloudformation:*
                - iam:PassRole
                - s3:GetObject
                - s3:PutObject
              Resource: '*'

# Cross-account artifact store
CrossAccountArtifactStore:
  Type: AWS::S3::Bucket
  Properties:
    BucketName: !Sub 'cross-account-artifacts-${AWS::AccountId}'
    ReplicationConfiguration:
      Role: !GetAtt S3ReplicationRole.Arn
      Rules:
        - Id: ReplicateToStaging
          Status: Enabled
          Prefix: staging/
          Destination:
            Bucket: !Sub 'arn:aws:s3:::staging-artifacts-${StagingAccountId}'
            StorageClass: STANDARD_IA
```

#### **Target Account IAM Configuration**
```yaml
# Dans le compte staging/production
CodePipelineServiceRole:
  Type: AWS::IAM::Role
  Properties:
    AssumeRolePolicyDocument:
      Version: '2012-10-17'
      Statement:
        - Effect: Allow
          Principal:
            AWS: !Sub 'arn:aws:iam::${MasterAccountId}:role/CodePipelineCrossAccountRole'
          Action: sts:AssumeRole
    Policies:
      - PolicyName: TargetAccountDeployment
        PolicyDocument:
          Version: '2012-10-17'
          Statement:
            - Effect: Allow
              Action:
                - cloudformation:CreateStack
                - cloudformation:UpdateStack
                - cloudformation:DeleteStack
                - cloudformation:DescribeStacks
                - cloudformation:CreateChangeSet
                - cloudformation:ExecuteChangeSet
                - iam:PassRole
              Resource: '*'
```

### **Cross-Region Pipeline Strategy**

#### **Multi-Region Artifact Replication**
```yaml
# Pipeline principal en us-east-1
PrimaryPipeline:
  Type: AWS::CodePipeline::Pipeline
  Properties:
    ArtifactStores:
      - Region: us-east-1
        ArtifactStore:
          Type: S3
          Location: !Ref PrimaryArtifactStore
          EncryptionKey:
            Id: !Ref PrimaryKMSKey
            Type: KMS
      - Region: eu-west-1
        ArtifactStore:
          Type: S3
          Location: !Ref SecondaryArtifactStore
          EncryptionKey:
            Id: !Ref SecondaryKMSKey
            Type: KMS
    
    Stages:
      # Deploy to multiple regions in parallel
      - Name: MultiRegionDeploy
        Actions:
          - Name: DeployUSEast1
            ActionTypeId:
              Category: Deploy
              Owner: AWS
              Provider: CloudFormation
              Version: 1
            Region: us-east-1
            Configuration:
              ActionMode: CREATE_UPDATE
              StackName: app-us-east-1
              TemplatePath: BuildOutput::template.yaml
            RunOrder: 1
            
          - Name: DeployEUWest1
            ActionTypeId:
              Category: Deploy
              Owner: AWS
              Provider: CloudFormation
              Version: 1
            Region: eu-west-1
            Configuration:
              ActionMode: CREATE_UPDATE
              StackName: app-eu-west-1
              TemplatePath: BuildOutput::template.yaml
            RunOrder: 1
```

---

## 🔄 **Advanced Pipeline Patterns**

### **Fan-Out Pattern (Parallel Environments)**

#### **Multi-Environment Deployment**
```yaml
# Stage avec déploiements parallèles
ParallelDeployStage:
  Name: ParallelDeploy
  Actions:
    - Name: DeployDev
      ActionTypeId:
        Category: Deploy
        Owner: AWS
        Provider: CodeDeploy
        Version: 1
      Configuration:
        ApplicationName: !Ref DevApplication
        DeploymentGroupName: dev-deployment-group
      InputArtifacts:
        - Name: BuildOutput
      RunOrder: 1
      
    - Name: DeployTest
      ActionTypeId:
        Category: Deploy
        Owner: AWS
        Provider: CodeDeploy
        Version: 1
      Configuration:
        ApplicationName: !Ref TestApplication
        DeploymentGroupName: test-deployment-group
      InputArtifacts:
        - Name: BuildOutput
      RunOrder: 1
      
    - Name: DeployDemo
      ActionTypeId:
        Category: Deploy
        Owner: AWS
        Provider: CodeDeploy
        Version: 1
      Configuration:
        ApplicationName: !Ref DemoApplication
        DeploymentGroupName: demo-deployment-group
      InputArtifacts:
        - Name: BuildOutput
      RunOrder: 1
```

### **Pipeline as Code avec CDK**

#### **CDK Pipeline Definition**
```typescript
import * as codepipeline from '@aws-cdk/aws-codepipeline';
import * as codepipeline_actions from '@aws-cdk/aws-codepipeline-actions';
import * as codebuild from '@aws-cdk/aws-codebuild';

export class DevOpsPipeline extends Construct {
  constructor(scope: Construct, id: string, props: DevOpsPipelineProps) {
    super(scope, id);

    // Source artifacts
    const sourceOutput = new codepipeline.Artifact('SourceOutput');
    const buildOutput = new codepipeline.Artifact('BuildOutput');

    // Build project
    const buildProject = new codebuild.Project(this, 'BuildProject', {
      environment: {
        buildImage: codebuild.LinuxBuildImage.STANDARD_5_0,
        privileged: true, // For Docker builds
      },
      buildSpec: codebuild.BuildSpec.fromObject({
        version: '0.2',
        phases: {
          install: {
            'runtime-versions': {
              nodejs: 16,
            },
          },
          pre_build: {
            commands: [
              'npm install',
              'npm test',
            ],
          },
          build: {
            commands: [
              'npm run build',
              'docker build -t $IMAGE_REPO_NAME:$IMAGE_TAG .',
            ],
          },
          post_build: {
            commands: [
              'echo Build completed on `date`',
            ],
          },
        },
        artifacts: {
          files: ['**/*'],
        },
      }),
    });

    // Pipeline
    const pipeline = new codepipeline.Pipeline(this, 'Pipeline', {
      pipelineName: `${props.appName}-pipeline`,
      stages: [
        {
          stageName: 'Source',
          actions: [
            new codepipeline_actions.CodeCommitSourceAction({
              actionName: 'CodeCommit',
              repository: props.repository,
              branch: props.branch,
              output: sourceOutput,
            }),
          ],
        },
        {
          stageName: 'Build',
          actions: [
            new codepipeline_actions.CodeBuildAction({
              actionName: 'CodeBuild',
              project: buildProject,
              input: sourceOutput,
              outputs: [buildOutput],
            }),
          ],
        },
        {
          stageName: 'Deploy',
          actions: [
            new codepipeline_actions.CloudFormationCreateUpdateStackAction({
              actionName: 'Deploy',
              templatePath: buildOutput.atPath('template.yaml'),
              stackName: `${props.appName}-stack`,
              adminPermissions: true,
            }),
          ],
        },
      ],
    });

    // EventBridge rule for auto-trigger
    new events.Rule(this, 'PipelineTriggerRule', {
      eventPattern: {
        source: ['aws.codecommit'],
        detailType: ['CodeCommit Repository State Change'],
        detail: {
          repositoryName: [props.repository.repositoryName],
          referenceName: [`refs/heads/${props.branch}`],
        },
      },
      targets: [new targets.CodePipeline(pipeline)],
    });
  }
}
```

---

## 📊 **Monitoring et Analytics**

### **Pipeline Metrics Dashboard**

#### **CloudWatch Dashboard pour Pipelines**
```yaml
PipelineDashboard:
  Type: AWS::CloudWatch::Dashboard
  Properties:
    DashboardName: CodePipeline-Analytics
    DashboardBody: !Sub |
      {
        "widgets": [
          {
            "type": "metric",
            "properties": {
              "metrics": [
                ["AWS/CodePipeline", "PipelineExecutionDuration", "PipelineName", "${CodePipeline}"],
                ["AWS/CodePipeline", "PipelineExecutionSuccess", "PipelineName", "${CodePipeline}"],
                ["AWS/CodePipeline", "PipelineExecutionFailure", "PipelineName", "${CodePipeline}"]
              ],
              "period": 300,
              "stat": "Sum",
              "region": "${AWS::Region}",
              "title": "Pipeline Executions"
            }
          },
          {
            "type": "metric",
            "properties": {
              "metrics": [
                ["AWS/CodeBuild", "Duration", "ProjectName", "${BuildProject}"],
                ["AWS/CodeBuild", "SucceededBuilds", "ProjectName", "${BuildProject}"],
                ["AWS/CodeBuild", "FailedBuilds", "ProjectName", "${BuildProject}"]
              ],
              "period": 300,
              "stat": "Average",
              "region": "${AWS::Region}",
              "title": "Build Performance"
            }
          }
        ]
      }
```

### **Pipeline Analytics avec Lambda**

#### **Custom Metrics Collection**
```python
import boto3
import json
from datetime import datetime, timedelta

def lambda_handler(event, context):
    """
    Collecte des métriques custom pour les pipelines
    """
    codepipeline = boto3.client('codepipeline')
    cloudwatch = boto3.client('cloudwatch')
    
    # Récupérer tous les pipelines
    pipelines = codepipeline.list_pipelines()['pipelines']
    
    for pipeline in pipelines:
        pipeline_name = pipeline['name']
        
        # Récupérer les exécutions récentes
        executions = codepipeline.list_pipeline_executions(
            pipelineName=pipeline_name,
            maxResults=10
        )['pipelineExecutions']
        
        # Calculer les métriques
        total_executions = len(executions)
        successful_executions = len([e for e in executions if e['status'] == 'Succeeded'])
        failed_executions = len([e for e in executions if e['status'] == 'Failed'])
        
        # Calculer le temps moyen d'exécution
        durations = []
        for execution in executions:
            if 'startTime' in execution and 'endTime' in execution:
                duration = (execution['endTime'] - execution['startTime']).total_seconds()
                durations.append(duration)
        
        avg_duration = sum(durations) / len(durations) if durations else 0
        
        # Publier les métriques custom
        metrics = [
            {
                'MetricName': 'ExecutionSuccessRate',
                'Value': (successful_executions / total_executions * 100) if total_executions > 0 else 0,
                'Unit': 'Percent'
            },
            {
                'MetricName': 'AverageExecutionDuration',
                'Value': avg_duration,
                'Unit': 'Seconds'
            },
            {
                'MetricName': 'FailureRate',
                'Value': (failed_executions / total_executions * 100) if total_executions > 0 else 0,
                'Unit': 'Percent'
            }
        ]
        
        for metric in metrics:
            cloudwatch.put_metric_data(
                Namespace='AWS/CodePipeline/Custom',
                MetricData=[
                    {
                        'MetricName': metric['MetricName'],
                        'Dimensions': [
                            {
                                'Name': 'PipelineName',
                                'Value': pipeline_name
                            }
                        ],
                        'Value': metric['Value'],
                        'Unit': metric['Unit']
                    }
                ]
            )
    
    return {
        'statusCode': 200,
        'body': json.dumps(f'Processed {len(pipelines)} pipelines')
    }
```

---

## ✅ **Quiz AWS CodePipeline**

### **Question 1:** Quelle est la différence entre des actions séquentielles et parallèles dans CodePipeline ?
<details>
<summary>Réponse</summary>

**Actions Séquentielles :**
- Exécutées l'une après l'autre
- Contrôlées par `RunOrder` (1, 2, 3...)
- La suivante attend la completion de la précédente
- **Exemple :** Build → Test → Deploy

**Actions Parallèles :**
- Exécutées simultanément
- Même `RunOrder` ou pas de RunOrder spécifié
- Toutes doivent réussir pour passer au stage suivant
- **Exemple :** Deploy simultané vers Dev, Test, et Demo

```yaml
ParallelActions:
  - Name: Action1
    RunOrder: 1  # Parallèle
  - Name: Action2
    RunOrder: 1  # Parallèle
  - Name: Action3
    RunOrder: 2  # Séquentiel après 1 et 2
```
</details>

### **Question 2:** Comment configurer un pipeline cross-account avec CodePipeline ?
<details>
<summary>Réponse</summary>

**Configuration Cross-Account :**

1. **Master Account (Pipeline):**
   - Service role avec permissions cross-account
   - S3 bucket policy permettant l'accès depuis target accounts

2. **Target Account:**
   - IAM role assumable par master account
   - Permissions pour déployer resources

3. **Pipeline Configuration:**
   ```yaml
   DeployAction:
     ActionTypeId:
       Category: Deploy
       Provider: CloudFormation
     Configuration:
       RoleArn: arn:aws:iam::TARGET-ACCOUNT:role/CrossAccountRole
       StackName: target-stack
   ```

**Best Practices :**
- Utiliser KMS keys séparées par account
- S3 bucket replication pour artifacts
- Least privilege IAM policies
- CloudTrail logging pour audit
</details>

### **Question 3:** Comment optimiser les performances d'un pipeline CodePipeline ?
<details>
<summary>Réponse</summary>

**Stratégies d'Optimisation :**

1. **Parallélisation :**
   - Actions parallèles dans stages
   - Build matrix pour différents environnements
   - Tests parallèles (unit, integration, security)

2. **Caching :**
   - CodeBuild cache pour dependencies
   - Docker layer caching
   - Artifact reuse entre stages

3. **Stage Optimization :**
   - Fail fast sur critical tests
   - Conditional actions based on changes
   - Smart triggers (webhooks vs polling)

4. **Infrastructure :**
   - Right-size compute resources
   - Use closest regions
   - Optimize Docker images

**Exemple :**
```yaml
BuildStage:
  Actions:
    - UnitTests (RunOrder: 1)
    - LintCheck (RunOrder: 1)  # Parallel
    - SecurityScan (RunOrder: 1)  # Parallel
    - Integration (RunOrder: 2)  # After all parallel
```
</details>

### **Question 4:** Comment implémenter des deployment gates avec CodePipeline ?
<details>
<summary>Réponse</summary>

**Types de Deployment Gates :**

1. **Manual Approval :**
   ```yaml
   ApprovalAction:
     ActionTypeId:
       Category: Approval
       Owner: AWS
       Provider: Manual
     Configuration:
       NotificationArn: !Ref SNSTopic
       CustomData: "Review deployment to production"
   ```

2. **Lambda Gates :**
   ```python
   def lambda_handler(event, context):
       # Check metrics, run tests, validate conditions
       if deployment_should_proceed():
           return {'status': 'SUCCEEDED'}
       else:
           return {'status': 'FAILED'}
   ```

3. **CloudWatch Alarms :**
   - Monitor application metrics
   - Auto-fail if thresholds exceeded
   - Integration with CodeDeploy rollback

4. **External Tool Integration :**
   - Quality gates from SonarQube
   - Security scans from third-party tools
   - Performance tests validation

**Recommended Pattern :** Combinaison de automated gates + manual approval pour production.
</details>

---

## 🎯 **Points Clés pour Certification AWS**

### **CodePipeline Core Features**
- **Visual workflow** avec stages et actions
- **Multi-source support** (CodeCommit, GitHub, S3, ECR)
- **Parallel et sequential execution** dans stages
- **Cross-account** et **cross-region** deployments
- **Integration** avec tous les services AWS et third-party

### **Best Practices**
- Use **CloudWatch Events** over polling for triggers
- Implement **deployment gates** and approval processes
- Configure **automatic rollback** mechanisms
- Use **parallel actions** to optimize pipeline speed
- Implement **cross-account** patterns for production
- Monitor **pipeline metrics** and set up alerting

### **Security & Compliance**
- **IAM roles** avec least privilege principle
- **KMS encryption** pour artifacts
- **CloudTrail** logging pour audit trail
- **VPC integration** pour private deployments
- **Secrets management** avec Parameter Store/Secrets Manager

---

**🎯 Next: AWS CloudFormation - Infrastructure as Code →**
