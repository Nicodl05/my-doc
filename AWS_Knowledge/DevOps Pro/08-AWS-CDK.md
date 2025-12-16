# 🚀 AWS CDK - Modern Infrastructure as Code
## Cloud Development Kit et Programmable Infrastructure

---

## 🎯 **AWS CDK Overview**

### **Qu'est-ce que AWS CDK ?**
**AWS Cloud Development Kit (CDK)** est un framework open-source qui permet de :
- **Définir l'infrastructure** avec des langages de programmation familiers
- **Générer des templates CloudFormation** automatiquement
- **Réutiliser des patterns** via des constructs prédéfinis
- **Tester l'infrastructure** avec des outils de testing standards
- **Intégrer** nativement avec les IDE et workflows de développement

### **CDK vs CloudFormation vs Terraform**
```yaml
AWS CDK:
  ✅ Langages familiers (TypeScript, Python, Java, C#)
  ✅ IntelliSense et type safety
  ✅ Constructs réutilisables
  ✅ Testing infrastructure possible
  ✅ Génère CloudFormation optimisé
  ❌ Courbe apprentissage pour ops teams

CloudFormation:
  ✅ Déclaratif et simple
  ✅ Familier pour ops teams
  ✅ Support AWS complet
  ❌ YAML/JSON verbeux
  ❌ Pas de réutilisabilité
  ❌ Pas de testing

Terraform:
  ✅ Multi-cloud
  ✅ HCL plus lisible que JSON
  ✅ Modules réutilisables
  ❌ État externe à gérer
  ❌ Moins d'intégration AWS native
```

---

## 🏗️ **CDK Architecture et Concepts**

### **Hiérarchie CDK**
```typescript
CDK App
├── Stack 1 (Production)
│   ├── Construct: VPC
│   ├── Construct: ECS Cluster
│   └── Construct: RDS Database
├── Stack 2 (Staging)
│   ├── Construct: VPC
│   └── Construct: Lambda Functions
└── Stack 3 (Shared Resources)
    ├── Construct: Route53 Hosted Zone
    └── Construct: Certificate Manager
```

### **Levels of Constructs**
```yaml
L1 Constructs (CFN Resources):
  - Mapping direct 1:1 avec CloudFormation
  - Exemple: CfnBucket, CfnInstance
  - Contrôle granulaire maximal
  - Verbeux, pas de best practices intégrées

L2 Constructs (Intent-based):
  - Abstractions avec best practices
  - Exemple: Bucket, Instance, Function
  - Reasonable defaults
  - Méthodes helper et validations

L3 Constructs (Patterns):
  - Solutions architecturales complètes
  - Exemple: ApplicationLoadBalancedEcsService
  - Multiple resources configurées ensemble
  - Opinionated mais flexible
```

### **Intégrations CDK avec Services AWS**

#### **Developer Tools Integration**
```yaml
CodeCommit:
  - Repository construct pour source control
  - Integration avec CodeBuild et CodePipeline
  - Trigger automatique sur commits

CodeBuild:
  - Project construct avec buildspec intégré
  - Environment management automatisé
  - Artifact handling simplifié

CodePipeline:
  - Pipeline construct avec stages
  - Source/Build/Deploy actions prédéfinies
  - Cross-account deployment patterns

CodeDeploy:
  - Application et DeploymentGroup constructs
  - Blue/Green deployment configurations
  - Integration avec ECS et Lambda
```

#### **Compute Services Integration**
```yaml
EC2:
  - Instance et SecurityGroup constructs
  - UserData et KeyPair management
  - Auto Scaling Group patterns

ECS:
  - Cluster, Service, TaskDefinition constructs
  - Fargate et EC2 launch types
  - Load balancer integration automatique

Lambda:
  - Function construct avec runtime management
  - Event source mapping automatique
  - Layer et environment variables

EKS:
  - Cluster construct avec node groups
  - Helm chart deployment
  - IRSA (IAM Roles for Service Accounts)
```

#### **Storage et Database Integration**
```yaml
S3:
  - Bucket construct avec encryption defaults
  - Notification configuration automatique
  - Cross-region replication setup

RDS:
  - DatabaseInstance et DatabaseCluster
  - Parameter groups et option groups
  - Backup et maintenance windows

DynamoDB:
  - Table construct avec billing modes
  - GSI et LSI configuration
  - Stream integration avec Lambda

EFS:
  - FileSystem construct avec mount targets
  - Access points configuration
  - Integration avec ECS et Lambda
```

#### **Networking Integration**
```yaml
VPC:
  - Vpc construct avec subnets automatiques
  - NAT Gateway et Internet Gateway
  - Route table configuration

ALB/NLB:
  - ApplicationLoadBalancer constructs
  - Target groups et health checks
  - SSL certificate integration

CloudFront:
  - Distribution construct avec origins
  - Cache behaviors et invalidation
  - WAF integration

Route53:
  - HostedZone et RecordSet constructs
  - Health check configuration
  - Alias record automation
```

#### **Security Services Integration**
```yaml
IAM:
  - Role, Policy, User constructs
  - Managed policy attachments
  - Cross-account trust relationships

Secrets Manager:
  - Secret construct avec rotation
  - Dynamic reference generation
  - Cross-service integration

Parameter Store:
  - StringParameter constructs
  - Hierarchical parameter organization
  - Secure string encryption

Certificate Manager:
  - Certificate construct avec validation
  - DNS et email validation methods
  - Multi-domain certificate support
```

#### **Monitoring et Observability**
```yaml
CloudWatch:
  - Alarm construct avec metric integration
  - Dashboard creation automatique
  - Log group management

X-Ray:
  - Tracing configuration pour Lambda/ECS
  - Service map visualization
  - Performance insights

SNS/SQS:
  - Topic et Queue constructs
  - Subscription management automatique
  - Dead letter queue configuration

EventBridge:
  - Rule construct avec event patterns
  - Target configuration automatique
  - Cross-account event routing
```

---

## 🔧 **CDK Best Practices et Patterns**

### **Construct Design Patterns**

#### **Environment Configuration Pattern**
```typescript
// Configuration par environnement
interface EnvironmentConfig {
  instanceType: ec2.InstanceType;
  minCapacity: number;
  maxCapacity: number;
  certificateArn?: string;
  domainName?: string;
}

const environmentConfigs: Record<string, EnvironmentConfig> = {
  development: {
    instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO),
    minCapacity: 1,
    maxCapacity: 2,
  },
  staging: {
    instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.SMALL),
    minCapacity: 2,
    maxCapacity: 4,
  },
  production: {
    instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MEDIUM),
    minCapacity: 3,
    maxCapacity: 10,
    certificateArn: 'arn:aws:acm:us-east-1:123456789012:certificate/abcd-1234',
    domainName: 'app.company.com',
  },
};
```

#### **Cross-Stack Reference Pattern**
```typescript
// Shared resources stack
export class NetworkStack extends Stack {
  public readonly vpc: ec2.Vpc;
  public readonly albSecurityGroup: ec2.SecurityGroup;

  constructor(scope: Construct, id: string, props: StackProps) {
    super(scope, id, props);

    this.vpc = new ec2.Vpc(this, 'VPC', {
      maxAzs: 2,
      subnetConfiguration: [
        { name: 'Public', subnetType: ec2.SubnetType.PUBLIC },
        { name: 'Private', subnetType: ec2.SubnetType.PRIVATE_WITH_NAT },
        { name: 'Database', subnetType: ec2.SubnetType.PRIVATE_ISOLATED },
      ],
    });

    // Export VPC ID for cross-stack reference
    new CfnOutput(this, 'VpcId', {
      value: this.vpc.vpcId,
      exportName: `${this.stackName}-vpc-id`,
    });
  }
}

// Application stack utilisant les ressources partagées
export class ApplicationStack extends Stack {
  constructor(scope: Construct, id: string, networkStack: NetworkStack, props: StackProps) {
    super(scope, id, props);

    // Utilisation directe de la référence
    const alb = new elbv2.ApplicationLoadBalancer(this, 'ALB', {
      vpc: networkStack.vpc, // Référence directe
      internetFacing: true,
    });

    // Ou import depuis export CloudFormation
    const importedVpc = ec2.Vpc.fromLookup(this, 'ImportedVpc', {
      vpcId: Fn.importValue(`${networkStack.stackName}-vpc-id`),
    });
  }
}
```

#### **Custom Construct Pattern**
```typescript
// Construct réutilisable pour application web
export interface WebApplicationProps {
  vpc: ec2.Vpc;
  domainName?: string;
  certificateArn?: string;
  minCapacity: number;
  maxCapacity: number;
  instanceType: ec2.InstanceType;
}

export class WebApplication extends Construct {
  public readonly loadBalancer: elbv2.ApplicationLoadBalancer;
  public readonly autoScalingGroup: autoscaling.AutoScalingGroup;

  constructor(scope: Construct, id: string, props: WebApplicationProps) {
    super(scope, id);

    // Security Groups
    const albSg = new ec2.SecurityGroup(this, 'AlbSecurityGroup', {
      vpc: props.vpc,
      description: 'Security group for ALB',
      allowAllOutbound: false,
    });

    albSg.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(80),
      'Allow HTTP traffic'
    );

    if (props.certificateArn) {
      albSg.addIngressRule(
        ec2.Peer.anyIpv4(),
        ec2.Port.tcp(443),
        'Allow HTTPS traffic'
      );
    }

    // Application Load Balancer
    this.loadBalancer = new elbv2.ApplicationLoadBalancer(this, 'ALB', {
      vpc: props.vpc,
      internetFacing: true,
      securityGroup: albSg,
    });

    // Auto Scaling Group avec Launch Template
    const launchTemplate = new ec2.LaunchTemplate(this, 'LaunchTemplate', {
      instanceType: props.instanceType,
      machineImage: ec2.MachineImage.latestAmazonLinux(),
      securityGroup: this.createWebServerSecurityGroup(props.vpc, albSg),
      userData: this.createUserData(),
      role: this.createInstanceRole(),
    });

    this.autoScalingGroup = new autoscaling.AutoScalingGroup(this, 'ASG', {
      vpc: props.vpc,
      launchTemplate,
      minCapacity: props.minCapacity,
      maxCapacity: props.maxCapacity,
      vpcSubnets: { subnetType: ec2.SubnetType.PRIVATE_WITH_NAT },
      healthCheck: autoscaling.HealthCheck.elb({
        grace: Duration.minutes(5),
      }),
    });

    // Target Group et Listener
    const targetGroup = new elbv2.ApplicationTargetGroup(this, 'TargetGroup', {
      vpc: props.vpc,
      port: 80,
      protocol: elbv2.ApplicationProtocol.HTTP,
      targets: [this.autoScalingGroup],
      healthCheck: {
        path: '/health',
        healthyHttpCodes: '200',
      },
    });

    this.loadBalancer.addListener('HttpListener', {
      port: 80,
      defaultTargetGroups: [targetGroup],
    });

    // HTTPS Listener si certificat fourni
    if (props.certificateArn) {
      const certificate = acm.Certificate.fromCertificateArn(
        this, 'Certificate', props.certificateArn
      );

      this.loadBalancer.addListener('HttpsListener', {
        port: 443,
        certificates: [certificate],
        defaultTargetGroups: [targetGroup],
      });
    }

    // Auto Scaling Policies
    this.autoScalingGroup.scaleOnCpuUtilization('CpuScaling', {
      targetUtilizationPercent: 70,
      scaleInCooldown: Duration.minutes(5),
      scaleOutCooldown: Duration.minutes(3),
    });

    // CloudWatch Alarms
    new cloudwatch.Alarm(this, 'HighCpuAlarm', {
      metric: this.autoScalingGroup.metricCpuUtilization(),
      threshold: 80,
      evaluationPeriods: 2,
      datapointsToAlarm: 2,
    });

    // Route 53 Record si domaine fourni
    if (props.domainName) {
      this.createDnsRecord(props.domainName);
    }
  }

  private createWebServerSecurityGroup(vpc: ec2.Vpc, albSg: ec2.SecurityGroup): ec2.SecurityGroup {
    const webSg = new ec2.SecurityGroup(this, 'WebServerSecurityGroup', {
      vpc,
      description: 'Security group for web servers',
      allowAllOutbound: true,
    });

    webSg.addIngressRule(
      albSg,
      ec2.Port.tcp(80),
      'Allow traffic from ALB'
    );

    return webSg;
  }

  private createUserData(): ec2.UserData {
    const userData = ec2.UserData.forLinux();
    userData.addCommands(
      'yum update -y',
      'yum install -y httpd',
      'systemctl start httpd',
      'systemctl enable httpd',
      'echo "<h1>Hello from $(hostname -f)</h1>" > /var/www/html/index.html',
      // Health check endpoint
      'echo "OK" > /var/www/html/health'
    );
    return userData;
  }

  private createInstanceRole(): iam.Role {
    return new iam.Role(this, 'InstanceRole', {
      assumedBy: new iam.ServicePrincipal('ec2.amazonaws.com'),
      managedPolicies: [
        iam.ManagedPolicy.fromAwsManagedPolicyName('CloudWatchAgentServerPolicy'),
        iam.ManagedPolicy.fromAwsManagedPolicyName('AmazonSSMManagedInstanceCore'),
      ],
    });
  }

  private createDnsRecord(domainName: string): void {
    const hostedZone = route53.HostedZone.fromLookup(this, 'HostedZone', {
      domainName: domainName.split('.').slice(-2).join('.'), // Extract root domain
    });

    new route53.ARecord(this, 'AliasRecord', {
      zone: hostedZone,
      recordName: domainName,
      target: route53.RecordTarget.fromAlias(
        new route53Targets.LoadBalancerTarget(this.loadBalancer)
      ),
    });
  }
}
```

---

## 🔄 **CDK Pipelines et CI/CD Integration**

### **CDK Pipeline Self-Mutating**
```typescript
// CDK Pipeline qui se déploie elle-même
export class CdkPipelineStack extends Stack {
  constructor(scope: Construct, id: string, props: StackProps) {
    super(scope, id, props);

    // Source depuis CodeCommit
    const sourceArtifact = new codepipeline.Artifact();
    const cloudAssemblyArtifact = new codepipeline.Artifact();

    const pipeline = new pipelines.CdkPipeline(this, 'Pipeline', {
      pipelineName: 'InfrastructurePipeline',
      cloudAssemblyArtifact,

      // Source stage
      sourceAction: new codepipeline_actions.CodeCommitSourceAction({
        actionName: 'CodeCommit',
        repository: codecommit.Repository.fromRepositoryName(
          this, 'Repo', 'infrastructure-repo'
        ),
        branch: 'main',
        output: sourceArtifact,
      }),

      // Build stage
      synthAction: pipelines.SimpleSynthAction.standardNpmSynth({
        sourceArtifact,
        cloudAssemblyArtifact,
        buildCommand: 'npm run build',
        synthCommand: 'npx cdk synth',
        environment: {
          privileged: true, // Pour Docker builds
        },
      }),
    });

    // Déploiement multi-environnement
    const devStage = new ApplicationStage(this, 'Development', {
      env: { account: '123456789012', region: 'us-east-1' },
      environmentName: 'development',
    });

    const prodStage = new ApplicationStage(this, 'Production', {
      env: { account: '987654321098', region: 'us-east-1' },
      environmentName: 'production',
    });

    // Ajout des stages au pipeline
    pipeline.addApplicationStage(devStage);
    
    // Stage production avec approval manuel
    pipeline.addApplicationStage(prodStage, {
      manualApprovals: true,
    });
  }
}

// Application Stage définissant l'infrastructure complète
export class ApplicationStage extends Stage {
  constructor(scope: Construct, id: string, props: ApplicationStageProps) {
    super(scope, id, props);

    // Network foundation
    const networkStack = new NetworkStack(this, 'Network', {
      environmentName: props.environmentName,
    });

    // Application infrastructure
    const appStack = new ApplicationStack(this, 'Application', {
      networkStack,
      environmentName: props.environmentName,
    });

    // Database infrastructure
    const dbStack = new DatabaseStack(this, 'Database', {
      networkStack,
      environmentName: props.environmentName,
    });

    // Dependencies
    appStack.addDependency(networkStack);
    dbStack.addDependency(networkStack);
  }
}
```

### **Integration avec CodeBuild et CodeDeploy**
```typescript
// CDK construisant et déployant une application
export class ApplicationDeploymentStack extends Stack {
  constructor(scope: Construct, id: string, props: StackProps) {
    super(scope, id, props);

    // CodeBuild project pour build application
    const buildProject = new codebuild.Project(this, 'BuildProject', {
      buildSpec: codebuild.BuildSpec.fromObject({
        version: '0.2',
        phases: {
          install: {
            'runtime-versions': {
              nodejs: '16',
            },
            commands: [
              'npm install -g aws-cdk',
              'npm install',
            ],
          },
          build: {
            commands: [
              'npm run test',
              'npm run build',
              'cdk synth',
            ],
          },
        },
        artifacts: {
          'base-directory': 'cdk.out',
          files: ['**/*'],
        },
      }),
      environment: {
        buildImage: codebuild.LinuxBuildImage.STANDARD_5_0,
        privileged: true,
      },
    });

    // CodeDeploy pour déploiement ECS
    const ecsApplication = new codedeploy.EcsApplication(this, 'EcsApplication');

    const deploymentGroup = new codedeploy.EcsDeploymentGroup(this, 'BlueGreenDG', {
      application: ecsApplication,
      deploymentConfig: codedeploy.EcsDeploymentConfig.CANARY_10PERCENT_5MINUTES,
      blueGreenDeploymentConfig: {
        listener: this.albListener,
        testListener: this.testListener,
        targetGroup: this.blueTargetGroup,
        deploymentApprovalWaitTime: Duration.minutes(5),
        terminationWaitTime: Duration.minutes(5),
      },
    });

    // Pipeline intégrant CDK, Build et Deploy
    new codepipeline.Pipeline(this, 'DeploymentPipeline', {
      stages: [
        {
          stageName: 'Source',
          actions: [/* Source action */],
        },
        {
          stageName: 'Build',
          actions: [
            new codepipeline_actions.CodeBuildAction({
              actionName: 'Build',
              project: buildProject,
              input: sourceOutput,
              outputs: [buildOutput],
            }),
          ],
        },
        {
          stageName: 'Deploy',
          actions: [
            new codepipeline_actions.CodeDeployEcsDeployAction({
              actionName: 'Deploy',
              deploymentGroup,
              appSpecTemplateInput: buildOutput,
              taskDefinitionTemplateInput: buildOutput,
            }),
          ],
        },
      ],
    });
  }
}
```

---

## 📊 **Testing et Validation Infrastructure**

### **Unit Testing CDK Constructs**
```typescript
// Jest tests pour CDK constructs
import { Template } from '@aws-cdk/assertions';
import { Stack } from '@aws-cdk/core';
import { WebApplication } from '../lib/web-application';

describe('WebApplication Construct', () => {
  let stack: Stack;
  let template: Template;

  beforeEach(() => {
    stack = new Stack();
    new WebApplication(stack, 'TestApp', {
      vpc: mockVpc,
      minCapacity: 1,
      maxCapacity: 3,
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO),
    });
    template = Template.fromStack(stack);
  });

  test('creates ALB with correct configuration', () => {
    template.hasResourceProperties('AWS::ElasticLoadBalancingV2::LoadBalancer', {
      Type: 'application',
      Scheme: 'internet-facing',
    });
  });

  test('creates Auto Scaling Group with correct capacity', () => {
    template.hasResourceProperties('AWS::AutoScaling::AutoScalingGroup', {
      MinSize: '1',
      MaxSize: '3',
    });
  });

  test('configures security groups correctly', () => {
    template.hasResourceProperties('AWS::EC2::SecurityGroup', {
      SecurityGroupIngress: [
        {
          IpProtocol: 'tcp',
          FromPort: 80,
          ToPort: 80,
          CidrIp: '0.0.0.0/0',
        },
      ],
    });
  });

  test('does not create HTTPS listener without certificate', () => {
    template.resourceCountIs('AWS::ElasticLoadBalancingV2::Listener', 1);
  });
});

// Integration tests
describe('WebApplication Integration', () => {
  test('deploys successfully in test environment', async () => {
    const app = new App();
    const stack = new Stack(app, 'TestStack');
    
    // Deploy to test account
    const result = await deployStack(stack);
    expect(result.status).toBe('CREATE_COMPLETE');
    
    // Validate endpoints
    const albDns = result.outputs['AlbDnsName'];
    const response = await fetch(`http://${albDns}/health`);
    expect(response.status).toBe(200);
  });
});
```

### **Policy Validation et Security**
```typescript
// CDK Aspects pour validation security
import { IAspect, IConstruct } from '@aws-cdk/core';
import { CfnSecurityGroup } from '@aws-cdk/aws-ec2';

export class SecurityGroupValidationAspect implements IAspect {
  visit(node: IConstruct): void {
    if (node instanceof CfnSecurityGroup) {
      const ingressRules = node.securityGroupIngress || [];
      
      ingressRules.forEach((rule: any) => {
        // Vérifier les règles 0.0.0.0/0
        if (rule.cidrIp === '0.0.0.0/0') {
          if (rule.fromPort !== 80 && rule.fromPort !== 443) {
            node.node.addError(
              `Security group ${node.node.id} allows ingress from 0.0.0.0/0 on port ${rule.fromPort}. ` +
              'Only ports 80 and 443 should be open to the world.'
            );
          }
        }

        // Vérifier SSH access
        if (rule.fromPort === 22 && rule.cidrIp === '0.0.0.0/0') {
          node.node.addWarning(
            `Security group ${node.node.id} allows SSH from anywhere. Consider restricting to specific IPs.`
          );
        }
      });
    }
  }
}

// Application de l'aspect
const app = new App();
const stack = new MyStack(app, 'MyStack');
stack.node.applyAspect(new SecurityGroupValidationAspect());
```

---

## 🔗 **Service Integrations Avancées**

### **CDK avec Step Functions**
```typescript
// Orchestration de pipeline DevOps avec Step Functions
export class DevOpsOrchestrationStack extends Stack {
  constructor(scope: Construct, id: string, props: StackProps) {
    super(scope, id, props);

    // Lambda functions pour chaque étape
    const buildFunction = new lambda.Function(this, 'BuildFunction', {
      runtime: lambda.Runtime.PYTHON_3_9,
      handler: 'index.handler',
      code: lambda.Code.fromInline(`
import boto3
import json

def handler(event, context):
    codebuild = boto3.client('codebuild')
    
    response = codebuild.start_build(
        projectName=event['buildProject'],
        sourceVersion=event['commitId']
    )
    
    return {
        'buildId': response['build']['id'],
        'status': 'STARTED'
    }
      `),
    });

    const testFunction = new lambda.Function(this, 'TestFunction', {
      runtime: lambda.Runtime.PYTHON_3_9,
      handler: 'index.handler',
      code: lambda.Code.fromInline(`
import boto3
import time

def handler(event, context):
    # Run integration tests
    # Check quality gates
    # Validate security scans
    
    return {
        'testResults': 'PASSED',
        'coverage': 85,
        'securityScan': 'CLEAN'
    }
      `),
    });

    const deployFunction = new lambda.Function(this, 'DeployFunction', {
      runtime: lambda.Runtime.PYTHON_3_9,
      handler: 'index.handler',
      code: lambda.Code.fromInline(`
import boto3

def handler(event, context):
    if event['environment'] == 'production':
        # Blue/Green deployment
        # Gradual traffic shifting
        # Health checks
        pass
    else:
        # Direct deployment for non-prod
        pass
    
    return {
        'deploymentStatus': 'SUCCESS',
        'endpoint': f"https://{event['environment']}.app.com"
    }
      `),
    });

    // Step Function State Machine
    const buildTask = new sfnTasks.LambdaInvoke(this, 'BuildTask', {
      lambdaFunction: buildFunction,
      outputPath: '$.Payload',
    });

    const testTask = new sfnTasks.LambdaInvoke(this, 'TestTask', {
      lambdaFunction: testFunction,
      outputPath: '$.Payload',
    });

    const deployTask = new sfnTasks.LambdaInvoke(this, 'DeployTask', {
      lambdaFunction: deployFunction,
      outputPath: '$.Payload',
    });

    // Parallel testing
    const parallelTests = new sfn.Parallel(this, 'ParallelTests')
      .branch(testTask)
      .branch(new sfnTasks.LambdaInvoke(this, 'SecurityScanTask', {
        lambdaFunction: /* security scan function */,
      }))
      .branch(new sfnTasks.LambdaInvoke(this, 'PerformanceTestTask', {
        lambdaFunction: /* performance test function */,
      }));

    // Choice state pour environnement
    const environmentChoice = new sfn.Choice(this, 'EnvironmentChoice')
      .when(sfn.Condition.stringEquals('$.environment', 'production'),
        new sfnTasks.SnsPublish(this, 'RequestApproval', {
          topic: approvalTopic,
          message: sfn.TaskInput.fromJsonPathAt('$'),
        }).next(deployTask)
      )
      .otherwise(deployTask);

    // Definition du workflow
    const definition = buildTask
      .next(parallelTests)
      .next(environmentChoice);

    new sfn.StateMachine(this, 'DevOpsStateMachine', {
      definition,
      timeout: Duration.hours(2),
    });
  }
}
```

### **CDK avec EventBridge**
```typescript
// Event-driven DevOps avec EventBridge
export class EventDrivenDevOpsStack extends Stack {
  constructor(scope: Construct, id: string, props: StackProps) {
    super(scope, id, props);

    // Custom EventBridge Bus pour DevOps events
    const devOpsBus = new events.EventBus(this, 'DevOpsBus', {
      eventBusName: 'devops-events',
    });

    // Rules pour différents events
    const commitRule = new events.Rule(this, 'CommitRule', {
      eventBus: devOpsBus,
      eventPattern: {
        source: ['codecommit'],
        detailType: ['CodeCommit Repository State Change'],
        detail: {
          event: ['pushToRepository'],
          repositoryName: ['main-app-repo'],
        },
      },
    });

    const buildCompleteRule = new events.Rule(this, 'BuildCompleteRule', {
      eventBus: devOpsBus,
      eventPattern: {
        source: ['codebuild'],
        detailType: ['CodeBuild Build State Change'],
        detail: {
          'build-status': ['SUCCEEDED'],
        },
      },
    });

    const deploymentFailureRule = new events.Rule(this, 'DeploymentFailureRule', {
      eventBus: devOpsBus,
      eventPattern: {
        source: ['codedeploy'],
        detailType: ['CodeDeploy Deployment State-change Notification'],
        detail: {
          state: ['FAILURE'],
        },
      },
    });

    // Targets pour actions automatiques
    commitRule.addTarget(new targets.CodePipeline(/* pipeline */));
    buildCompleteRule.addTarget(new targets.LambdaFunction(/* notify function */));
    deploymentFailureRule.addTarget(new targets.LambdaFunction(/* rollback function */));

    // Cross-account event routing
    devOpsBus.addToResourcePolicy(new iam.PolicyStatement({
      sid: 'AllowCrossAccountEvents',
      effect: iam.Effect.ALLOW,
      principals: [
        new iam.AccountPrincipal('123456789012'), // Dev account
        new iam.AccountPrincipal('987654321098'), // Prod account
      ],
      actions: ['events:PutEvents'],
      resources: [devOpsBus.eventBusArn],
    }));
  }
}
```

---

## ✅ **Quiz AWS CDK**

### **Question 1:** Quelles sont les différences entre L1, L2, et L3 constructs dans CDK ?
<details>
<summary>Réponse</summary>

**L1 Constructs (CFN Resources) :**
- **Mapping 1:1** avec CloudFormation resources
- **Préfixe Cfn** (CfnBucket, CfnInstance)
- **Contrôle granulaire** maximal
- **Pas de best practices** intégrées
- **Plus verbeux** mais plus flexible

**L2 Constructs (Intent-based) :**
- **Abstractions** avec reasonable defaults
- **Best practices** AWS intégrées
- **Méthodes helper** et validations
- **Exemple :** `new s3.Bucket()` avec encryption par défaut
- **Balance** entre simplicité et contrôle

**L3 Constructs (Patterns) :**
- **Solutions architecturales** complètes
- **Plusieurs resources** configurées ensemble
- **Highly opinionated** avec patterns proven
- **Exemple :** `ApplicationLoadBalancedEcsService`
- **Productivité maximale** pour cas d'usage standards

**Best Practice :** Commencer avec L2/L3, descendre vers L1 pour besoins spécifiques.
</details>

### **Question 2:** Comment gérer les environnements multiples avec CDK ?
<details>
<summary>Réponse</summary>

**Stratégies Multi-Environment :**

1. **Configuration Objects :**
```typescript
const envConfigs = {
  dev: { instanceType: 't3.micro', minCapacity: 1 },
  prod: { instanceType: 't3.large', minCapacity: 3 }
};
```

2. **Environment-specific Stacks :**
```typescript
new MyStack(app, 'DevStack', { env: { account: '123', region: 'us-east-1' }});
new MyStack(app, 'ProdStack', { env: { account: '456', region: 'us-east-1' }});
```

3. **CDK Pipelines avec Stages :**
```typescript
pipeline.addApplicationStage(new DevStage(app, 'Dev'));
pipeline.addApplicationStage(new ProdStage(app, 'Prod'), {
  manualApprovals: true
});
```

4. **Context Variables :**
```typescript
const environment = this.node.tryGetContext('environment') || 'dev';
const config = envConfigs[environment];
```

**Best Practices :**
- Separate AWS accounts pour environments
- Configuration externalisée
- Environment-specific deployment pipelines
- Resource tagging par environment
</details>

### **Question 3:** Quels services AWS s'intègrent nativement avec CDK pour DevOps ?
<details>
<summary>Réponse</summary>

**Developer Tools Integration :**
- **CodeCommit :** Repository constructs avec triggers
- **CodeBuild :** Project constructs avec buildspec intégré
- **CodePipeline :** Self-mutating pipelines
- **CodeDeploy :** Blue/Green deployment configurations

**Compute & Container Services :**
- **ECS/Fargate :** ApplicationLoadBalancedFargateService
- **EKS :** Cluster constructs avec node groups
- **Lambda :** Function constructs avec event sources
- **EC2 :** Instance et AutoScaling patterns

**Monitoring & Observability :**
- **CloudWatch :** Alarms, Dashboards, Logs
- **X-Ray :** Tracing configuration automatique
- **SNS/SQS :** Event-driven architectures
- **EventBridge :** Rule-based automation

**Security & Governance :**
- **IAM :** Role et Policy management
- **Secrets Manager :** Secret rotation
- **Certificate Manager :** SSL/TLS automation
- **Config :** Compliance rules

**Storage & Database :**
- **S3 :** Bucket constructs avec notifications
- **RDS :** Database clusters avec backups
- **DynamoDB :** Tables avec streams
- **EFS :** File systems avec mount targets
</details>

### **Question 4:** Comment implémenter testing et validation avec CDK ?
<details>
<summary>Réponse</summary>

**Types de Testing :**

1. **Unit Tests :**
```typescript
// Test des constructs individuels
import { Template } from '@aws-cdk/assertions';
template.hasResourceProperties('AWS::S3::Bucket', {
  PublicReadAccess: false
});
```

2. **Integration Tests :**
```typescript
// Test end-to-end avec déploiement réel
const result = await deployStack(testStack);
expect(result.status).toBe('CREATE_COMPLETE');
```

3. **Security Validation :**
```typescript
// CDK Aspects pour policy enforcement
class SecurityAspect implements IAspect {
  visit(node: IConstruct): void {
    // Validate security groups, IAM policies, etc.
  }
}
```

4. **Snapshot Testing :**
```typescript
// Vérifier que CloudFormation output n'a pas changé
expect(Template.fromStack(stack).toJSON()).toMatchSnapshot();
```

**Validation Tools :**
- **cdk diff :** Preview changes
- **cfn-lint :** CloudFormation validation
- **cdk-nag :** Security et compliance rules
- **AWS Config :** Runtime compliance checking

**CI/CD Integration :**
- Tests automatiques dans pipeline
- Quality gates avant deployment
- Rollback automatique sur échec
- Cross-environment validation
</details>

---

## 🎯 **Points Clés pour Certification AWS**

### **CDK Core Concepts**
- **Constructs hierarchy** (L1, L2, L3) et quand utiliser chaque niveau
- **Cross-stack references** et dependency management
- **Environment configuration** et multi-account patterns
- **CDK Pipelines** pour self-mutating infrastructure

### **DevOps Integration**
- **Native integration** avec Developer Tools (CodeCommit, CodeBuild, CodePipeline, CodeDeploy)
- **Event-driven automation** avec EventBridge et Lambda
- **Blue/Green deployments** avec Route 53 et Load Balancers
- **Cross-account deployment** patterns et IAM roles

### **Best Practices**
- **Testing infrastructure** avec unit tests et integration tests
- **Security validation** avec CDK Aspects et policy enforcement
- **Monitoring integration** avec CloudWatch et X-Ray
- **Reusable patterns** via custom constructs et construct libraries

### **Service Interactions**
- **Compute :** EC2, ECS, Lambda, EKS intégration seamless
- **Storage :** S3, RDS, DynamoDB avec automated configurations
- **Networking :** VPC, ALB, Route 53 avec best practices defaults
- **Security :** IAM, Secrets Manager, Certificate Manager automation

---

**🎯 Next: Amazon ECR/ECS - Container Registry et Orchestration →**
