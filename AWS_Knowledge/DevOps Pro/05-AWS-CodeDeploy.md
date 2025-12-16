# 🚀 AWS CodeDeploy - Deployment Automation
## Déploiements Automatisés et Stratégies de Rollback

---

## 🎯 **AWS CodeDeploy Overview**

### **Qu'est-ce que CodeDeploy ?**
**AWS CodeDeploy** est un service de déploiement entièrement managé qui :
- Automatise les déploiements d'applications
- Supporte plusieurs stratégies de déploiement
- Minimise les temps d'arrêt
- Fournit des capacités de rollback automatique
- S'intègre avec EC2, Lambda, ECS et on-premises

### **Plateformes Supportées**
```yaml
Compute Platforms:
  EC2/On-Premises:
    - Instances EC2
    - Serveurs on-premises 
    - Auto Scaling groups
    - Stratégies: In-place, Blue/Green
    
  AWS Lambda:
    - Functions Lambda
    - Alias et versions
    - Stratégies: Linear, Canary, All-at-once
    
  Amazon ECS:
    - Services ECS
    - Task definitions
    - Stratégies: Blue/Green
```

---

## 🏗️ **Architecture et Concepts CodeDeploy**

### **Composants Principaux**

#### **Application et Deployment Groups**
```yaml
Application:
  - Container logique pour déploiements
  - Spécifie la plateforme (EC2, Lambda, ECS)
  - Peut avoir plusieurs deployment groups

Deployment Group:
  - Collection de targets pour déploiement
  - Définit les instances/resources à déployer
  - Configure les stratégies de déploiement
  - Spécifie les alarmes et rollback rules

Deployment Configuration:
  - Prédéfinie AWS ou custom
  - Définit comment le déploiement s'exécute
  - Contrôle la vitesse et les méthodes
```

#### **CodeDeploy Agent**
```bash
# Installation sur EC2 Amazon Linux
sudo yum update
sudo yum install ruby wget
cd /home/ec2-user
wget https://aws-codedeploy-us-east-1.s3.us-east-1.amazonaws.com/latest/install
chmod +x ./install
sudo ./install auto

# Vérifier le statut
sudo service codedeploy-agent status

# Auto-start au boot
sudo chkconfig codedeploy-agent on
```

---

## 🔄 **Stratégies de Déploiement**

### **1. EC2/On-Premises Deployments**

#### **In-Place Deployment**
```yaml
Characteristics:
  - Déploiement sur instances existantes
  - Downtime durant le déploiement
  - Rollback = redéploiement version précédente
  - Économique (pas de nouvelles instances)

Process:
  1. Stop application
  2. Deploy new version
  3. Start application
  4. Validate deployment

Use Cases:
  - Environments de développement
  - Applications avec maintenance windows
  - Contraintes de coût importantes
```

#### **Blue/Green Deployment**
```yaml
Characteristics:
  - Nouvelles instances (Green) créées
  - Traffic switchover instantané
  - Zero downtime possible
  - Rollback immédiat (switch back)
  - Plus coûteux (double instances temporairement)

Process:
  1. Create new instances (Green)
  2. Deploy application to Green
  3. Test Green environment
  4. Switch traffic from Blue to Green
  5. Terminate Blue instances (after validation)

Use Cases:
  - Production environments
  - Applications critiques
  - Zero-downtime requirements
```

### **2. Lambda Deployments**

#### **Linear Deployment**
```yaml
Linear10PercentEvery1Minute:
  - 10% du traffic shifted chaque minute
  - Déploiement graduel sur 10 minutes
  - Détection d'erreurs progressive

Linear10PercentEvery2Minutes:
  - 10% du traffic shifted toutes les 2 minutes
  - Déploiement plus conservateur
  - Plus de temps pour détecter les problèmes

Linear10PercentEvery3Minutes:
  - Version encore plus conservative
  - Idéal pour applications critiques
```

#### **Canary Deployment**
```yaml
Canary10Percent5Minutes:
  - 10% du traffic immédiatement
  - Attendre 5 minutes
  - Puis 100% si pas d'alarmes

Canary10Percent10Minutes:
  - 10% puis attendre 10 minutes
  - Validation plus longue

Canary10Percent15Minutes:
  - Validation extended 15 minutes
  - Maximum de précaution
```

### **3. ECS Deployments**

#### **Blue/Green avec ECS**
```yaml
Process:
  1. Create new task definition
  2. Create new ECS service (Green)
  3. Update load balancer target groups
  4. Shift traffic progressively
  5. Monitor health checks
  6. Complete switch or rollback

Benefits:
  - Zero downtime
  - Full rollback capability
  - Container-native deployment
  - Integration avec ALB/NLB
```

---

## 📝 **AppSpec File Configuration**

### **AppSpec pour EC2/On-Premises**

#### **Structure AppSpec.yml**
```yaml
version: 0.0
os: linux

files:
  - source: /
    destination: /var/www/html
    overwrite: true
  - source: /config/
    destination: /etc/myapp/
    overwrite: false

permissions:
  - object: /var/www/html
    pattern: "**"
    owner: apache
    group: apache
    mode: 755
    type:
      - file
  - object: /var/www/html/bin
    owner: root
    mode: 755
    type:
      - directory

hooks:
  BeforeInstall:
    - location: scripts/install_dependencies.sh
      timeout: 300
      runas: root
    - location: scripts/backup_current.sh
      timeout: 600
      runas: root
      
  AfterInstall:
    - location: scripts/change_permissions.sh
      timeout: 300
      runas: root
      
  ApplicationStart:
    - location: scripts/start_server.sh
      timeout: 300
      runas: root
    - location: scripts/run_migrations.sh
      timeout: 900
      runas: app_user
      
  ApplicationStop:
    - location: scripts/stop_server.sh
      timeout: 300
      runas: root
      
  ValidateService:
    - location: scripts/validate_service.sh
      timeout: 300
      runas: ec2-user
```

#### **Lifecycle Event Hooks**
```bash
# scripts/install_dependencies.sh
#!/bin/bash
yum update -y
yum install -y httpd
systemctl enable httpd

# scripts/start_server.sh  
#!/bin/bash
systemctl start httpd
systemctl status httpd

# scripts/validate_service.sh
#!/bin/bash
# Vérifier que l'application répond
curl -f http://localhost:80/health
if [ $? -eq 0 ]; then
    echo "Application is healthy"
    exit 0
else
    echo "Application health check failed"
    exit 1
fi

# scripts/stop_server.sh
#!/bin/bash
systemctl stop httpd
```

### **AppSpec pour Lambda**

#### **AppSpec.yaml pour Lambda**
```yaml
version: 0.0
Resources:
  - MyLambdaFunction:
      Type: AWS::Lambda::Function
      Properties:
        Name: "my-function"
        Alias: "live"
        CurrentVersion: "1"
        TargetVersion: "2"
        
Hooks:
  BeforeAllowTraffic:
    - BeforeAllowTrafficHook
  AfterAllowTraffic:
    - AfterAllowTrafficHook
```

#### **Validation Hooks Lambda**
```python
import boto3
import json

def before_allow_traffic_hook(event, context):
    """
    Hook exécuté avant de diriger le traffic vers la nouvelle version
    """
    lambda_client = boto3.client('lambda')
    
    # Récupérer les détails du déploiement
    deployment_id = event['DeploymentId']
    lifecycle_event_hook_execution_id = event['LifecycleEventHookExecutionId']
    
    try:
        # Exécuter tests de validation
        function_name = event['DeploymentId']['LambdaTarget']['LambdaFunctionName']
        
        # Invoquer la nouvelle version pour tests
        response = lambda_client.invoke(
            FunctionName=function_name,
            Payload=json.dumps({'test': True})
        )
        
        # Valider la réponse
        if response['StatusCode'] == 200:
            # Signaler succès à CodeDeploy
            codedeploy = boto3.client('codedeploy')
            codedeploy.put_lifecycle_event_hook_execution_status(
                deploymentId=deployment_id,
                lifecycleEventHookExecutionId=lifecycle_event_hook_execution_id,
                status='Succeeded'
            )
        else:
            raise Exception("Function validation failed")
            
    except Exception as e:
        # Signaler échec à CodeDeploy
        codedeploy = boto3.client('codedeploy')
        codedeploy.put_lifecycle_event_hook_execution_status(
            deploymentId=deployment_id,
            lifecycleEventHookExecutionId=lifecycle_event_hook_execution_id,
            status='Failed'
        )
        raise e

def after_allow_traffic_hook(event, context):
    """
    Hook exécuté après avoir dirigé le traffic
    """
    # Nettoyage, notifications, métriques
    print("Deployment completed successfully")
    
    # Envoyer notification
    sns = boto3.client('sns')
    sns.publish(
        TopicArn='arn:aws:sns:us-east-1:123456789012:deployment-notifications',
        Message=f"Lambda deployment completed: {event['DeploymentId']}"
    )
```

---

## 🔧 **Configuration via CloudFormation**

### **CodeDeploy Application et Deployment Group**

#### **EC2 Application Setup**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'CodeDeploy Application for EC2'

Parameters:
  ApplicationName:
    Type: String
    Default: my-web-app
    
  EnvironmentName:
    Type: String
    Default: production

Resources:
  # CodeDeploy Application
  CodeDeployApplication:
    Type: AWS::CodeDeploy::Application
    Properties:
      ApplicationName: !Ref ApplicationName
      ComputePlatform: Server  # Server = EC2/On-premises

  # Service Role pour CodeDeploy
  CodeDeployServiceRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: codedeploy.amazonaws.com
            Action: sts:AssumeRole
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/service-role/AWSCodeDeployRole
      Policies:
        - PolicyName: CodeDeployAutoScalingPolicy
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - autoscaling:CompleteLifecycleAction
                  - autoscaling:DeleteLifecycleHook
                  - autoscaling:DescribeLifecycleHooks
                  - autoscaling:DescribeAutoScalingGroups
                  - autoscaling:PutLifecycleHook
                  - autoscaling:RecordLifecycleActionHeartbeat
                  - ec2:CreateTags
                  - ec2:DescribeInstances
                Resource: '*'

  # Deployment Group
  CodeDeployDeploymentGroup:
    Type: AWS::CodeDeploy::DeploymentGroup
    Properties:
      ApplicationName: !Ref CodeDeployApplication
      DeploymentGroupName: !Sub '${EnvironmentName}-deployment-group'
      ServiceRoleArn: !GetAtt CodeDeployServiceRole.Arn
      DeploymentConfigName: CodeDeployDefault.AllAtOneBlueGreen
      AutoRollbackConfiguration:
        Enabled: true
        Events:
          - DEPLOYMENT_FAILURE
          - DEPLOYMENT_STOP_ON_ALARM
          - DEPLOYMENT_STOP_ON_INSTANCE_FAILURE
      AutoScalingGroups:
        - !Ref AutoScalingGroup
      LoadBalancerInfo:
        TargetGroupInfoList:
          - Name: !Ref TargetGroup
      AlarmConfiguration:
        Enabled: true
        Alarms:
          - Name: !Ref HighErrorRateAlarm
      BlueGreenDeploymentConfiguration:
        TerminateBlueInstancesOnDeploymentSuccess:
          Action: TERMINATE
          TerminationWaitTimeInMinutes: 5
        DeploymentReadyOption:
          ActionOnTimeout: CONTINUE_DEPLOYMENT
        GreenFleetProvisioningOption:
          Action: COPY_AUTO_SCALING_GROUP

  # CloudWatch Alarm pour rollback automatique
  HighErrorRateAlarm:
    Type: AWS::CloudWatch::Alarm
    Properties:
      AlarmName: !Sub '${ApplicationName}-high-error-rate'
      AlarmDescription: 'High error rate detected'
      MetricName: 4XXError
      Namespace: AWS/ApplicationELB
      Statistic: Sum
      Period: 300
      EvaluationPeriods: 2
      Threshold: 10
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: LoadBalancer
          Value: !GetAtt LoadBalancer.LoadBalancerFullName

Outputs:
  ApplicationName:
    Description: 'CodeDeploy Application Name'
    Value: !Ref CodeDeployApplication
    Export:
      Name: !Sub '${AWS::StackName}-app-name'
      
  DeploymentGroupName:
    Description: 'CodeDeploy Deployment Group Name'
    Value: !Ref CodeDeployDeploymentGroup
    Export:
      Name: !Sub '${AWS::StackName}-deployment-group'
```

### **Lambda Application Setup**

#### **Lambda CodeDeploy Configuration**
```yaml
LambdaCodeDeployApplication:
  Type: AWS::CodeDeploy::Application
  Properties:
    ApplicationName: my-lambda-app
    ComputePlatform: Lambda

LambdaDeploymentGroup:
  Type: AWS::CodeDeploy::DeploymentGroup
  Properties:
    ApplicationName: !Ref LambdaCodeDeployApplication
    DeploymentGroupName: lambda-deployment-group
    ServiceRoleArn: !GetAtt LambdaCodeDeployRole.Arn
    DeploymentConfigName: CodeDeployDefault.LambdaCanary10Percent5Minutes
    AutoRollbackConfiguration:
      Enabled: true
      Events:
        - DEPLOYMENT_FAILURE
        - DEPLOYMENT_STOP_ON_ALARM
    AlarmConfiguration:
      Enabled: true
      Alarms:
        - Name: !Ref LambdaErrorAlarm
        - Name: !Ref LambdaDurationAlarm

# Alarmes pour rollback automatique
LambdaErrorAlarm:
  Type: AWS::CloudWatch::Alarm
  Properties:
    AlarmName: lambda-error-rate
    MetricName: Errors
    Namespace: AWS/Lambda
    Statistic: Sum
    Period: 300
    EvaluationPeriods: 2
    Threshold: 5
    ComparisonOperator: GreaterThanThreshold
    Dimensions:
      - Name: FunctionName
        Value: !Ref LambdaFunction

LambdaDurationAlarm:
  Type: AWS::CloudWatch::Alarm
  Properties:
    AlarmName: lambda-duration
    MetricName: Duration
    Namespace: AWS/Lambda
    Statistic: Average
    Period: 300
    EvaluationPeriods: 2
    Threshold: 10000  # 10 seconds
    ComparisonOperator: GreaterThanThreshold
    Dimensions:
      - Name: FunctionName
        Value: !Ref LambdaFunction
```

---

## 🔄 **Rollback et Recovery Strategies**

### **Automatic Rollback Configuration**

#### **CloudWatch-Based Rollback**
```yaml
AutoRollbackConfiguration:
  Enabled: true
  Events:
    - DEPLOYMENT_FAILURE          # Échec technique du déploiement
    - DEPLOYMENT_STOP_ON_ALARM    # Alarme CloudWatch déclenchée
    - DEPLOYMENT_STOP_ON_INSTANCE_FAILURE  # Échec sur instances

AlarmConfiguration:
  Enabled: true
  IgnorePollAlarmFailure: false  # Rollback si alarm polling échoue
  Alarms:
    - Name: ApplicationErrorRate
    - Name: ResponseTime
    - Name: CPUUtilization
```

#### **Custom Rollback Logic**
```python
import boto3
import json

def deployment_monitor(event, context):
    """
    Fonction Lambda pour monitoring custom et rollback
    """
    codedeploy = boto3.client('codedeploy')
    cloudwatch = boto3.client('cloudwatch')
    
    deployment_id = event['deployment_id']
    
    # Récupérer le statut du déploiement
    response = codedeploy.get_deployment(deploymentId=deployment_id)
    deployment = response['deploymentInfo']
    
    if deployment['status'] in ['InProgress', 'Queued']:
        # Vérifier métriques custom
        if check_custom_metrics():
            print("Custom metrics are healthy")
        else:
            print("Custom metrics indicate problems, stopping deployment")
            
            # Arrêter le déploiement
            codedeploy.stop_deployment(
                deploymentId=deployment_id,
                autoRollbackEnabled=True
            )
    
def check_custom_metrics():
    """
    Vérifier des métriques business custom
    """
    cloudwatch = boto3.client('cloudwatch')
    
    # Exemple: vérifier le taux de conversion
    response = cloudwatch.get_metric_statistics(
        Namespace='Business/Application',
        MetricName='ConversionRate',
        StartTime=datetime.utcnow() - timedelta(minutes=10),
        EndTime=datetime.utcnow(),
        Period=300,
        Statistics=['Average']
    )
    
    if response['Datapoints']:
        current_rate = response['Datapoints'][-1]['Average']
        return current_rate > 0.02  # 2% minimum conversion rate
    
    return False
```

### **Blue/Green Rollback Process**

#### **Immediate Rollback Strategy**
```bash
#!/bin/bash
# Rollback script pour Blue/Green deployment

DEPLOYMENT_ID=$1
APPLICATION_NAME=$2
DEPLOYMENT_GROUP=$3

echo "Starting rollback for deployment: $DEPLOYMENT_ID"

# Arrêter le déploiement en cours
aws codedeploy stop-deployment \
    --deployment-id $DEPLOYMENT_ID \
    --auto-rollback-enabled

# Vérifier que le rollback est initié
ROLLBACK_STATUS=$(aws codedeploy get-deployment \
    --deployment-id $DEPLOYMENT_ID \
    --query 'deploymentInfo.status' \
    --output text)

if [ "$ROLLBACK_STATUS" = "Stopped" ]; then
    echo "Rollback initiated successfully"
    
    # Attendre la completion du rollback
    aws codedeploy wait deployment-successful \
        --deployment-id $DEPLOYMENT_ID
        
    echo "Rollback completed"
else
    echo "Failed to initiate rollback"
    exit 1
fi
```

---

## 📊 **Monitoring et Metrics**

### **CodeDeploy CloudWatch Metrics**

#### **Key Metrics à Surveiller**
```yaml
Deployment Metrics:
  - Deployments: Nombre de déploiements
  - DeploymentTime: Durée des déploiements
  - FailedDeployments: Déploiements échoués
  - SuccessfulDeployments: Déploiements réussis

Instance Metrics:
  - InstanceHealth: Santé des instances
  - InstanceSuccess: Instances déployées avec succès
  - InstanceFailure: Instances avec échec de déploiement

Application Metrics:
  - ApplicationAvailability: Disponibilité application
  - ErrorRate: Taux d'erreurs post-déploiement
  - ResponseTime: Temps de réponse application
```

#### **Custom Metrics Dashboard**
```yaml
CodeDeployDashboard:
  Type: AWS::CloudWatch::Dashboard
  Properties:
    DashboardName: CodeDeploy-Monitoring
    DashboardBody: !Sub |
      {
        "widgets": [
          {
            "type": "metric",
            "properties": {
              "metrics": [
                ["AWS/CodeDeploy", "Deployments", "ApplicationName", "${ApplicationName}"],
                ["AWS/CodeDeploy", "FailedDeployments", "ApplicationName", "${ApplicationName}"]
              ],
              "period": 300,
              "stat": "Sum",
              "region": "${AWS::Region}",
              "title": "Deployment Overview"
            }
          },
          {
            "type": "metric", 
            "properties": {
              "metrics": [
                ["AWS/ApplicationELB", "TargetResponseTime", "LoadBalancer", "${LoadBalancer}"],
                ["AWS/ApplicationELB", "HTTPCode_Target_4XX_Count", "LoadBalancer", "${LoadBalancer}"],
                ["AWS/ApplicationELB", "HTTPCode_Target_5XX_Count", "LoadBalancer", "${LoadBalancer}"]
              ],
              "period": 300,
              "stat": "Average",
              "region": "${AWS::Region}",
              "title": "Application Health"
            }
          }
        ]
      }
```

---

## ✅ **Quiz AWS CodeDeploy**

### **Question 1:** Quelle est la différence entre In-Place et Blue/Green deployment ?
<details>
<summary>Réponse</summary>

**In-Place Deployment :**
- Déploie sur les instances existantes
- Downtime pendant le déploiement
- Rollback = redéploiement de l'ancienne version
- Plus économique (pas de nouvelles instances)
- **Usage :** Dev/staging environments

**Blue/Green Deployment :**
- Crée de nouvelles instances (Green)
- Switch instantané du traffic
- Zero downtime possible
- Rollback immédiat (switch back)
- Plus coûteux temporairement
- **Usage :** Production environments

**Recommandation AWS :** Blue/Green pour production, In-Place pour dev/test.
</details>

### **Question 2:** Comment configurer un rollback automatique avec CloudWatch alarms ?
<details>
<summary>Réponse</summary>

**Configuration Rollback Automatique :**

```yaml
DeploymentGroup:
  AutoRollbackConfiguration:
    Enabled: true
    Events:
      - DEPLOYMENT_FAILURE
      - DEPLOYMENT_STOP_ON_ALARM
      - DEPLOYMENT_STOP_ON_INSTANCE_FAILURE
  AlarmConfiguration:
    Enabled: true
    IgnorePollAlarmFailure: false
    Alarms:
      - Name: HighErrorRate
      - Name: HighResponseTime
```

**Alarmes recommandées :**
- **Error Rate** : > 5% HTTP 5xx errors
- **Response Time** : > 2x baseline
- **CPU/Memory** : > 80% utilization
- **Custom Business Metrics** : Conversion rate, etc.

**Trigger :** Dès qu'une alarme passe en état ALARM, rollback automatique.
</details>

### **Question 3:** Comment déployer une Lambda function avec canary deployment ?
<details>
<summary>Réponse</summary>

**Configuration Canary Lambda :**

```yaml
LambdaDeploymentGroup:
  Properties:
    DeploymentConfigName: CodeDeployDefault.LambdaCanary10Percent5Minutes
    
AppSpec.yaml:
  version: 0.0
  Resources:
    - MyFunction:
        Type: AWS::Lambda::Function
        Properties:
          Name: "my-function"
          Alias: "live"
          CurrentVersion: "1"
          TargetVersion: "2"
  Hooks:
    BeforeAllowTraffic:
      - ValidationFunction
```

**Process :**
1. 10% traffic → nouvelle version
2. Wait 5 minutes
3. Monitor CloudWatch alarms
4. Si OK → 100% traffic
5. Si KO → rollback automatique

**Validation Hook :** Function Lambda qui teste la nouvelle version avant full deployment.
</details>

### **Question 4:** Comment intégrer CodeDeploy avec Auto Scaling Groups ?
<details>
<summary>Réponse</summary>

**Configuration ASG avec CodeDeploy :**

```yaml
DeploymentGroup:
  Properties:
    AutoScalingGroups:
      - !Ref MyAutoScalingGroup
    BlueGreenDeploymentConfiguration:
      TerminateBlueInstancesOnDeploymentSuccess:
        Action: TERMINATE
        TerminationWaitTimeInMinutes: 5
      GreenFleetProvisioningOption:
        Action: COPY_AUTO_SCALING_GROUP
```

**Process Blue/Green avec ASG :**
1. CodeDeploy copie l'ASG (Green fleet)
2. Deploy application sur Green instances
3. Switch load balancer vers Green
4. Terminate Blue instances après validation

**Avantages :**
- Zero downtime
- Rollback immédiat possible
- Scale events handled automatically
- Health checks integrated
</details>

---

## 🎯 **Points Clés pour Certification AWS**

### **CodeDeploy Core Features**
- **Multiple deployment strategies** (In-place, Blue/Green, Canary)
- **Multi-platform support** (EC2, Lambda, ECS, On-premises)
- **Automatic rollback** based on CloudWatch alarms
- **Integration** with Auto Scaling Groups et Load Balancers
- **Monitoring** avec CloudWatch et X-Ray

### **Best Practices**
- Use **Blue/Green** for production zero-downtime deployments
- Configure **automatic rollback** with appropriate alarms
- Implement **health checks** in AppSpec lifecycle hooks
- Use **deployment configurations** appropriate for your risk tolerance
- Monitor **deployment metrics** and application health
- Test **rollback procedures** regularly

### **Security & Compliance**
- **IAM roles** with least privilege for CodeDeploy service
- **Encryption** of deployment artifacts
- **VPC integration** for private deployments
- **CloudTrail** logging for audit compliance
- **Tags** for resource management and cost allocation

---

**🎯 Next: AWS CodePipeline - Orchestration CI/CD →**
