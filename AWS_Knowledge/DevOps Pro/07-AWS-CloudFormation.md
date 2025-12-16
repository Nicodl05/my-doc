# 🏗️ AWS CloudFormation - Infrastructure as Code
## Templates, Stacks et Automation Avancée

---

## 🎯 **AWS CloudFormation Overview**

### **Qu'est-ce que CloudFormation ?**
**AWS CloudFormation** est le service d'Infrastructure as Code (IaC) natif d'AWS qui :
- Déploie et gère les ressources AWS via des templates
- Assure la cohérence et reproductibilité des infrastructures
- Gère les dépendances entre ressources automatiquement
- Fournit rollback automatique en cas d'échec
- Support des updates avec change sets
- Intégration native avec tous les services AWS

### **Avantages vs Terraform/Pulumi**
```yaml
AWS CloudFormation:
  ✅ Intégration native AWS
  ✅ Support complet services AWS
  ✅ Rollback automatique
  ✅ Change sets preview
  ✅ Drift detection
  ✅ Aucune infrastructure à gérer

Terraform:
  ✅ Multi-cloud support
  ✅ Syntaxe HCL plus lisible
  ✅ État distribué
  ❌ Moins d'intégration AWS native
  ❌ Rollback manuel

Pulumi:
  ✅ Langages de programmation
  ✅ Testing capabilities
  ❌ Courbe d'apprentissage
  ❌ Écosystème plus petit
```

---

## 📋 **Template Structure et Concepts**

### **Template Anatomy**

#### **Complete Template Structure**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Complete DevOps Infrastructure Template'

Metadata:
  AWS::CloudFormation::Interface:
    ParameterGroups:
      - Label:
          default: "Network Configuration"
        Parameters:
          - VpcCidr
          - PrivateSubnetCidr
          - PublicSubnetCidr
      - Label:
          default: "Application Configuration"
        Parameters:
          - InstanceType
          - KeyPair
          - EnvironmentType

Transform:
  - AWS::Serverless-2016-10-31  # SAM Transform
  - AWS::LanguageExtensions     # Intrinsic functions

Parameters:
  EnvironmentType:
    Type: String
    Default: development
    AllowedValues:
      - development
      - staging
      - production
    Description: Environment type for resource sizing

  VpcCidr:
    Type: String
    Default: 10.0.0.0/16
    AllowedPattern: '^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\/([0-9]|[1-2][0-9]|3[0-2])$'

  InstanceType:
    Type: String
    Default: t3.micro
    AllowedValues:
      - t3.micro
      - t3.small
      - t3.medium
      - t3.large

Mappings:
  EnvironmentMap:
    development:
      InstanceType: t3.micro
      MinSize: 1
      MaxSize: 2
      DesiredCapacity: 1
    staging:
      InstanceType: t3.small
      MinSize: 1
      MaxSize: 3
      DesiredCapacity: 2
    production:
      InstanceType: t3.medium
      MinSize: 2
      MaxSize: 10
      DesiredCapacity: 3

  RegionMap:
    us-east-1:
      AMI: ami-0abcdef1234567890
    us-west-2:
      AMI: ami-0fedcba0987654321
    eu-west-1:
      AMI: ami-0123456789abcdef0

Conditions:
  IsProduction: !Equals [!Ref EnvironmentType, 'production']
  IsNotProduction: !Not [!Condition IsProduction]
  CreateSSLCertificate: !And 
    - !Condition IsProduction
    - !Not [!Equals [!Ref DomainName, '']]

Resources:
  # Définis ci-dessous

Outputs:
  VpcId:
    Description: 'ID of the VPC'
    Value: !Ref VPC
    Export:
      Name: !Sub '${AWS::StackName}-vpc-id'
```

### **Advanced Functions et Expressions**

#### **Intrinsic Functions Avancées**
```yaml
# String manipulation
FormattedResourceName: !Sub 
  - '${Environment}-${Application}-${Component}-${Random}'
  - Environment: !Ref EnvironmentType
    Application: myapp
    Component: api
    Random: !Select [0, !Split ['-', !Select [2, !Split ['/', !Ref 'AWS::StackId']]]]

# Conditional resource creation
ConditionalResource: !If 
  - IsProduction
  - Type: AWS::RDS::DBInstance
    Properties:
      MultiAZ: true
      BackupRetentionPeriod: 30
  - Type: AWS::RDS::DBInstance
    Properties:
      MultiAZ: false
      BackupRetentionPeriod: 7

# Dynamic lists
AvailabilityZones: !GetAZs !Ref 'AWS::Region'
SubnetAZs: !If
  - IsProduction
  - !GetAZs !Ref 'AWS::Region'
  - [!Select [0, !GetAZs !Ref 'AWS::Region'], !Select [1, !GetAZs !Ref 'AWS::Region']]

# Transform functions (avec AWS::LanguageExtensions)
DynamicSecurityGroupRules: !ForEach
  - Port
  - [80, 443, 8080]
  - IpProtocol: tcp
    FromPort: !Ref Port
    ToPort: !Ref Port
    CidrIp: 0.0.0.0/0
```

---

## 🌐 **Multi-Tier Infrastructure Template**

### **Complete VPC avec Multi-AZ Setup**

#### **Network Foundation Template**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Multi-AZ VPC with Public/Private Subnets for DevOps'

Parameters:
  VpcCidr:
    Type: String
    Default: 10.0.0.0/16
  
  Environment:
    Type: String
    Default: development
    AllowedValues: [development, staging, production]

Mappings:
  SubnetConfig:
    VPC:
      CIDR: 10.0.0.0/16
    PublicSubnetAZ1:
      CIDR: 10.0.1.0/24
    PublicSubnetAZ2:
      CIDR: 10.0.2.0/24
    PrivateSubnetAZ1:
      CIDR: 10.0.11.0/24
    PrivateSubnetAZ2:
      CIDR: 10.0.12.0/24
    DatabaseSubnetAZ1:
      CIDR: 10.0.21.0/24
    DatabaseSubnetAZ2:
      CIDR: 10.0.22.0/24

Conditions:
  IsProduction: !Equals [!Ref Environment, 'production']

Resources:
  # VPC
  VPC:
    Type: AWS::EC2::VPC
    Properties:
      CidrBlock: !FindInMap [SubnetConfig, VPC, CIDR]
      EnableDnsHostnames: true
      EnableDnsSupport: true
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-vpc'
        - Key: Environment
          Value: !Ref Environment

  # Internet Gateway
  InternetGateway:
    Type: AWS::EC2::InternetGateway
    Properties:
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-igw'

  InternetGatewayAttachment:
    Type: AWS::EC2::VPCGatewayAttachment
    Properties:
      InternetGatewayId: !Ref InternetGateway
      VpcId: !Ref VPC

  # Public Subnets
  PublicSubnet1:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      AvailabilityZone: !Select [0, !GetAZs '']
      CidrBlock: !FindInMap [SubnetConfig, PublicSubnetAZ1, CIDR]
      MapPublicIpOnLaunch: true
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-public-subnet-az1'
        - Key: Type
          Value: Public

  PublicSubnet2:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      AvailabilityZone: !Select [1, !GetAZs '']
      CidrBlock: !FindInMap [SubnetConfig, PublicSubnetAZ2, CIDR]
      MapPublicIpOnLaunch: true
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-public-subnet-az2'
        - Key: Type
          Value: Public

  # NAT Gateways
  NatGateway1EIP:
    Type: AWS::EC2::EIP
    DependsOn: InternetGatewayAttachment
    Properties:
      Domain: vpc
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-nat-eip-az1'

  NatGateway2EIP:
    Type: AWS::EC2::EIP
    DependsOn: InternetGatewayAttachment
    Condition: IsProduction  # Only in production for HA
    Properties:
      Domain: vpc
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-nat-eip-az2'

  NatGateway1:
    Type: AWS::EC2::NatGateway
    Properties:
      AllocationId: !GetAtt NatGateway1EIP.AllocationId
      SubnetId: !Ref PublicSubnet1
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-nat-az1'

  NatGateway2:
    Type: AWS::EC2::NatGateway
    Condition: IsProduction
    Properties:
      AllocationId: !GetAtt NatGateway2EIP.AllocationId
      SubnetId: !Ref PublicSubnet2
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-nat-az2'

  # Private Subnets
  PrivateSubnet1:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      AvailabilityZone: !Select [0, !GetAZs '']
      CidrBlock: !FindInMap [SubnetConfig, PrivateSubnetAZ1, CIDR]
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-private-subnet-az1'
        - Key: Type
          Value: Private

  PrivateSubnet2:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      AvailabilityZone: !Select [1, !GetAZs '']
      CidrBlock: !FindInMap [SubnetConfig, PrivateSubnetAZ2, CIDR]
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-private-subnet-az2'
        - Key: Type
          Value: Private

  # Database Subnets
  DatabaseSubnet1:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      AvailabilityZone: !Select [0, !GetAZs '']
      CidrBlock: !FindInMap [SubnetConfig, DatabaseSubnetAZ1, CIDR]
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-database-subnet-az1'
        - Key: Type
          Value: Database

  DatabaseSubnet2:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref VPC
      AvailabilityZone: !Select [1, !GetAZs '']
      CidrBlock: !FindInMap [SubnetConfig, DatabaseSubnetAZ2, CIDR]
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-database-subnet-az2'
        - Key: Type
          Value: Database

  # Route Tables
  PublicRouteTable:
    Type: AWS::EC2::RouteTable
    Properties:
      VpcId: !Ref VPC
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-public-rt'

  DefaultPublicRoute:
    Type: AWS::EC2::Route
    DependsOn: InternetGatewayAttachment
    Properties:
      RouteTableId: !Ref PublicRouteTable
      DestinationCidrBlock: 0.0.0.0/0
      GatewayId: !Ref InternetGateway

  PublicSubnet1RouteTableAssociation:
    Type: AWS::EC2::SubnetRouteTableAssociation
    Properties:
      RouteTableId: !Ref PublicRouteTable
      SubnetId: !Ref PublicSubnet1

  PublicSubnet2RouteTableAssociation:
    Type: AWS::EC2::SubnetRouteTableAssociation
    Properties:
      RouteTableId: !Ref PublicRouteTable
      SubnetId: !Ref PublicSubnet2

  # Private Route Tables
  PrivateRouteTable1:
    Type: AWS::EC2::RouteTable
    Properties:
      VpcId: !Ref VPC
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-private-rt-az1'

  DefaultPrivateRoute1:
    Type: AWS::EC2::Route
    Properties:
      RouteTableId: !Ref PrivateRouteTable1
      DestinationCidrBlock: 0.0.0.0/0
      NatGatewayId: !Ref NatGateway1

  PrivateSubnet1RouteTableAssociation:
    Type: AWS::EC2::SubnetRouteTableAssociation
    Properties:
      RouteTableId: !Ref PrivateRouteTable1
      SubnetId: !Ref PrivateSubnet1

  PrivateRouteTable2:
    Type: AWS::EC2::RouteTable
    Properties:
      VpcId: !Ref VPC
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-private-rt-az2'

  DefaultPrivateRoute2:
    Type: AWS::EC2::Route
    Properties:
      RouteTableId: !Ref PrivateRouteTable2
      DestinationCidrBlock: 0.0.0.0/0
      NatGatewayId: !If 
        - IsProduction
        - !Ref NatGateway2
        - !Ref NatGateway1  # Use same NAT for dev/staging

  PrivateSubnet2RouteTableAssociation:
    Type: AWS::EC2::SubnetRouteTableAssociation
    Properties:
      RouteTableId: !Ref PrivateRouteTable2
      SubnetId: !Ref PrivateSubnet2

  # Database Subnet Group
  DatabaseSubnetGroup:
    Type: AWS::RDS::DBSubnetGroup
    Properties:
      DBSubnetGroupDescription: Subnet group for RDS database
      SubnetIds:
        - !Ref DatabaseSubnet1
        - !Ref DatabaseSubnet2
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-db-subnet-group'

  # Security Groups
  LoadBalancerSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupName: !Sub '${Environment}-alb-sg'
      GroupDescription: Security group for Application Load Balancer
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 80
          ToPort: 80
          CidrIp: 0.0.0.0/0
        - IpProtocol: tcp
          FromPort: 443
          ToPort: 443
          CidrIp: 0.0.0.0/0
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-alb-sg'

  WebServerSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupName: !Sub '${Environment}-web-sg'
      GroupDescription: Security group for web servers
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 80
          ToPort: 80
          SourceSecurityGroupId: !Ref LoadBalancerSecurityGroup
        - IpProtocol: tcp
          FromPort: 443
          ToPort: 443
          SourceSecurityGroupId: !Ref LoadBalancerSecurityGroup
        - IpProtocol: tcp
          FromPort: 22
          ToPort: 22
          SourceSecurityGroupId: !Ref BastionSecurityGroup
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-web-sg'

  DatabaseSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupName: !Sub '${Environment}-db-sg'
      GroupDescription: Security group for database
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 3306
          ToPort: 3306
          SourceSecurityGroupId: !Ref WebServerSecurityGroup
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-db-sg'

  BastionSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupName: !Sub '${Environment}-bastion-sg'
      GroupDescription: Security group for bastion host
      VpcId: !Ref VPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 22
          ToPort: 22
          CidrIp: 0.0.0.0/0  # Restrict to your IP in production
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-bastion-sg'

Outputs:
  VPC:
    Description: A reference to the created VPC
    Value: !Ref VPC
    Export:
      Name: !Sub '${AWS::StackName}-vpc-id'

  PublicSubnets:
    Description: A list of the public subnets
    Value: !Join [',', [!Ref PublicSubnet1, !Ref PublicSubnet2]]
    Export:
      Name: !Sub '${AWS::StackName}-public-subnets'

  PrivateSubnets:
    Description: A list of the private subnets
    Value: !Join [',', [!Ref PrivateSubnet1, !Ref PrivateSubnet2]]
    Export:
      Name: !Sub '${AWS::StackName}-private-subnets'

  DatabaseSubnets:
    Description: A list of the database subnets
    Value: !Join [',', [!Ref DatabaseSubnet1, !Ref DatabaseSubnet2]]
    Export:
      Name: !Sub '${AWS::StackName}-database-subnets'

  DatabaseSubnetGroup:
    Description: Database subnet group
    Value: !Ref DatabaseSubnetGroup
    Export:
      Name: !Sub '${AWS::StackName}-db-subnet-group'

  LoadBalancerSecurityGroup:
    Description: Security group for load balancer
    Value: !Ref LoadBalancerSecurityGroup
    Export:
      Name: !Sub '${AWS::StackName}-alb-sg'

  WebServerSecurityGroup:
    Description: Security group for web servers
    Value: !Ref WebServerSecurityGroup
    Export:
      Name: !Sub '${AWS::StackName}-web-sg'

  DatabaseSecurityGroup:
    Description: Security group for database
    Value: !Ref DatabaseSecurityGroup
    Export:
      Name: !Sub '${AWS::StackName}-db-sg'
```

---

## 🚀 **Application Infrastructure Template**

### **Auto Scaling Group avec ALB**

#### **Compute Layer Template**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Auto Scaling Web Application with ALB'

Parameters:
  NetworkStackName:
    Type: String
    Description: Name of the network stack to import values from

  KeyPairName:
    Type: AWS::EC2::KeyPair::KeyName
    Description: EC2 Key Pair for SSH access

  InstanceType:
    Type: String
    Default: t3.micro
    AllowedValues: [t3.micro, t3.small, t3.medium, t3.large]

  Environment:
    Type: String
    Default: development
    AllowedValues: [development, staging, production]

  ApplicationVersion:
    Type: String
    Default: 'latest'
    Description: Application version to deploy

Mappings:
  AmiMap:
    us-east-1:
      AMI: ami-0abcdef1234567890
    us-west-2:
      AMI: ami-0fedcba0987654321
    eu-west-1:
      AMI: ami-0123456789abcdef0

  EnvironmentMap:
    development:
      MinSize: 1
      MaxSize: 2
      DesiredCapacity: 1
      InstanceType: t3.micro
    staging:
      MinSize: 2
      MaxSize: 4
      DesiredCapacity: 2
      InstanceType: t3.small
    production:
      MinSize: 3
      MaxSize: 10
      DesiredCapacity: 3
      InstanceType: t3.medium

Resources:
  # Launch Template
  LaunchTemplate:
    Type: AWS::EC2::LaunchTemplate
    Properties:
      LaunchTemplateName: !Sub '${Environment}-web-server-lt'
      LaunchTemplateData:
        ImageId: !FindInMap [AmiMap, !Ref 'AWS::Region', AMI]
        InstanceType: !FindInMap [EnvironmentMap, !Ref Environment, InstanceType]
        KeyName: !Ref KeyPairName
        SecurityGroupIds:
          - Fn::ImportValue: !Sub '${NetworkStackName}-web-sg'
        IamInstanceProfile:
          Arn: !GetAtt InstanceProfile.Arn
        UserData:
          Fn::Base64: !Sub |
            #!/bin/bash
            yum update -y
            yum install -y httpd
            yum install -y aws-cli
            
            # Install CloudWatch agent
            wget https://s3.amazonaws.com/amazoncloudwatch-agent/amazon_linux/amd64/latest/amazon-cloudwatch-agent.rpm
            rpm -U ./amazon-cloudwatch-agent.rpm
            
            # Configure CloudWatch agent
            cat > /opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json << 'EOF'
            {
              "metrics": {
                "namespace": "CustomApp/${Environment}",
                "metrics_collected": {
                  "cpu": {
                    "measurement": ["cpu_usage_idle", "cpu_usage_iowait"]
                  },
                  "disk": {
                    "measurement": ["used_percent"],
                    "resources": ["*"]
                  },
                  "mem": {
                    "measurement": ["mem_used_percent"]
                  }
                }
              },
              "logs": {
                "logs_collected": {
                  "files": {
                    "collect_list": [
                      {
                        "file_path": "/var/log/httpd/access_log",
                        "log_group_name": "/aws/ec2/${Environment}/httpd/access",
                        "log_stream_name": "{instance_id}/access.log"
                      },
                      {
                        "file_path": "/var/log/httpd/error_log",
                        "log_group_name": "/aws/ec2/${Environment}/httpd/error",
                        "log_stream_name": "{instance_id}/error.log"
                      }
                    ]
                  }
                }
              }
            }
            EOF
            
            # Start CloudWatch agent
            /opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl \
              -a fetch-config -m ec2 -c file:/opt/aws/amazon-cloudwatch-agent/etc/amazon-cloudwatch-agent.json -s
            
            # Configure web server
            systemctl start httpd
            systemctl enable httpd
            
            # Deploy application
            aws s3 cp s3://${ApplicationBucket}/releases/${ApplicationVersion}/app.tar.gz /tmp/
            cd /var/www/html
            tar -xzf /tmp/app.tar.gz
            
            # Configure environment-specific settings
            echo "Environment: ${Environment}" > /var/www/html/env.txt
            echo "Version: ${ApplicationVersion}" >> /var/www/html/env.txt
            echo "Instance: $(curl -s http://169.254.169.254/latest/meta-data/instance-id)" >> /var/www/html/env.txt
            
            # Signal CloudFormation
            /opt/aws/bin/cfn-signal -e $? --stack ${AWS::StackName} --resource AutoScalingGroup --region ${AWS::Region}
        TagSpecifications:
          - ResourceType: instance
            Tags:
              - Key: Name
                Value: !Sub '${Environment}-web-server'
              - Key: Environment
                Value: !Ref Environment
              - Key: Application
                Value: WebApp

  # IAM Role for EC2 instances
  InstanceRole:
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
        - arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy
        - arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore
      Policies:
        - PolicyName: ApplicationAccess
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - s3:GetObject
                  - s3:ListBucket
                Resource:
                  - !Sub '${ApplicationBucket}/*'
                  - !GetAtt ApplicationBucket.Arn
              - Effect: Allow
                Action:
                  - logs:CreateLogGroup
                  - logs:CreateLogStream
                  - logs:PutLogEvents
                Resource: '*'

  InstanceProfile:
    Type: AWS::IAM::InstanceProfile
    Properties:
      Roles:
        - !Ref InstanceRole

  # Application Load Balancer
  ApplicationLoadBalancer:
    Type: AWS::ElasticLoadBalancingV2::LoadBalancer
    Properties:
      Name: !Sub '${Environment}-alb'
      Scheme: internet-facing
      Type: application
      Subnets:
        - Fn::Select:
          - 0
          - Fn::Split:
            - ','
            - Fn::ImportValue: !Sub '${NetworkStackName}-public-subnets'
        - Fn::Select:
          - 1
          - Fn::Split:
            - ','
            - Fn::ImportValue: !Sub '${NetworkStackName}-public-subnets'
      SecurityGroups:
        - Fn::ImportValue: !Sub '${NetworkStackName}-alb-sg'
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-alb'
        - Key: Environment
          Value: !Ref Environment

  # Target Group
  TargetGroup:
    Type: AWS::ElasticLoadBalancingV2::TargetGroup
    Properties:
      Name: !Sub '${Environment}-tg'
      Port: 80
      Protocol: HTTP
      VpcId:
        Fn::ImportValue: !Sub '${NetworkStackName}-vpc-id'
      HealthCheckPath: '/health'
      HealthCheckProtocol: HTTP
      HealthCheckIntervalSeconds: 30
      HealthCheckTimeoutSeconds: 5
      HealthyThresholdCount: 2
      UnhealthyThresholdCount: 3
      TargetGroupAttributes:
        - Key: deregistration_delay.timeout_seconds
          Value: '60'
        - Key: stickiness.enabled
          Value: 'true'
        - Key: stickiness.lb_cookie.duration_seconds
          Value: '86400'
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-tg'

  # ALB Listener
  ALBListener:
    Type: AWS::ElasticLoadBalancingV2::Listener
    Properties:
      DefaultActions:
        - Type: forward
          TargetGroupArn: !Ref TargetGroup
      LoadBalancerArn: !Ref ApplicationLoadBalancer
      Port: 80
      Protocol: HTTP

  # Auto Scaling Group
  AutoScalingGroup:
    Type: AWS::AutoScaling::AutoScalingGroup
    Properties:
      AutoScalingGroupName: !Sub '${Environment}-asg'
      LaunchTemplate:
        LaunchTemplateId: !Ref LaunchTemplate
        Version: !GetAtt LaunchTemplate.LatestVersionNumber
      MinSize: !FindInMap [EnvironmentMap, !Ref Environment, MinSize]
      MaxSize: !FindInMap [EnvironmentMap, !Ref Environment, MaxSize]
      DesiredCapacity: !FindInMap [EnvironmentMap, !Ref Environment, DesiredCapacity]
      VPCZoneIdentifier:
        - Fn::Select:
          - 0
          - Fn::Split:
            - ','
            - Fn::ImportValue: !Sub '${NetworkStackName}-private-subnets'
        - Fn::Select:
          - 1
          - Fn::Split:
            - ','
            - Fn::ImportValue: !Sub '${NetworkStackName}-private-subnets'
      TargetGroupARNs:
        - !Ref TargetGroup
      HealthCheckType: ELB
      HealthCheckGracePeriod: 300
      DefaultCooldown: 300
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-asg'
          PropagateAtLaunch: false
        - Key: Environment
          Value: !Ref Environment
          PropagateAtLaunch: true
    CreationPolicy:
      ResourceSignal:
        Count: !FindInMap [EnvironmentMap, !Ref Environment, DesiredCapacity]
        Timeout: PT15M
    UpdatePolicy:
      AutoScalingRollingUpdate:
        MinInstancesInService: 1
        MaxBatchSize: 1
        PauseTime: PT15M
        WaitOnResourceSignals: true

  # Scaling Policies
  ScaleUpPolicy:
    Type: AWS::AutoScaling::ScalingPolicy
    Properties:
      AdjustmentType: ChangeInCapacity
      AutoScalingGroupName: !Ref AutoScalingGroup
      Cooldown: 300
      ScalingAdjustment: 1

  ScaleDownPolicy:
    Type: AWS::AutoScaling::ScalingPolicy
    Properties:
      AdjustmentType: ChangeInCapacity
      AutoScalingGroupName: !Ref AutoScalingGroup
      Cooldown: 300
      ScalingAdjustment: -1

  # CloudWatch Alarms
  CPUAlarmHigh:
    Type: AWS::CloudWatch::Alarm
    Properties:
      AlarmName: !Sub '${Environment}-cpu-high'
      AlarmDescription: Scale up on high CPU
      MetricName: CPUUtilization
      Namespace: AWS/EC2
      Statistic: Average
      Period: 300
      EvaluationPeriods: 2
      Threshold: 80
      ComparisonOperator: GreaterThanThreshold
      Dimensions:
        - Name: AutoScalingGroupName
          Value: !Ref AutoScalingGroup
      AlarmActions:
        - !Ref ScaleUpPolicy
        - !Ref SNSAlert

  CPUAlarmLow:
    Type: AWS::CloudWatch::Alarm
    Properties:
      AlarmName: !Sub '${Environment}-cpu-low'
      AlarmDescription: Scale down on low CPU
      MetricName: CPUUtilization
      Namespace: AWS/EC2
      Statistic: Average
      Period: 300
      EvaluationPeriods: 2
      Threshold: 20
      ComparisonOperator: LessThanThreshold
      Dimensions:
        - Name: AutoScalingGroupName
          Value: !Ref AutoScalingGroup
      AlarmActions:
        - !Ref ScaleDownPolicy

  # S3 Bucket for application artifacts
  ApplicationBucket:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Sub '${Environment}-app-artifacts-${AWS::AccountId}'
      VersioningConfiguration:
        Status: Enabled
      BucketEncryption:
        ServerSideEncryptionConfiguration:
          - ServerSideEncryptionByDefault:
              SSEAlgorithm: AES256
      Tags:
        - Key: Name
          Value: !Sub '${Environment}-app-artifacts'
        - Key: Environment
          Value: !Ref Environment

  # SNS Topic for alerts
  SNSAlert:
    Type: AWS::SNS::Topic
    Properties:
      TopicName: !Sub '${Environment}-alerts'
      DisplayName: !Sub '${Environment} Application Alerts'

Outputs:
  LoadBalancerDNS:
    Description: DNS name of the load balancer
    Value: !GetAtt ApplicationLoadBalancer.DNSName
    Export:
      Name: !Sub '${AWS::StackName}-alb-dns'

  AutoScalingGroup:
    Description: Auto Scaling Group name
    Value: !Ref AutoScalingGroup
    Export:
      Name: !Sub '${AWS::StackName}-asg'

  ApplicationBucket:
    Description: S3 bucket for application artifacts
    Value: !Ref ApplicationBucket
    Export:
      Name: !Sub '${AWS::StackName}-app-bucket'
```

---

## 🔄 **Change Sets et Deployment Strategies**

### **Blue/Green Deployment avec CloudFormation**

#### **Blue/Green Stack Pattern**
```yaml
# blue-green-deployment.yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Blue/Green Deployment Pattern'

Parameters:
  Environment:
    Type: String
    AllowedValues: [blue, green]
    
  TargetEnvironment:
    Type: String
    Description: Environment to deploy to
    
  NetworkStackName:
    Type: String

Conditions:
  IsBlueEnvironment: !Equals [!Ref Environment, 'blue']
  IsGreenEnvironment: !Equals [!Ref Environment, 'green']

Resources:
  # Application Stack (Blue or Green)
  ApplicationStack:
    Type: AWS::CloudFormation::Stack
    Properties:
      TemplateURL: !Sub 
        - 'https://s3.amazonaws.com/templates/${Environment}-application-template.yaml'
        - Environment: !Ref Environment
      Parameters:
        Environment: !Ref Environment
        NetworkStackName: !Ref NetworkStackName
      Tags:
        - Key: Environment
          Value: !Ref Environment
        - Key: DeploymentType
          Value: BlueGreen

  # Route 53 Weighted Routing
  BlueRecord:
    Type: AWS::Route53::RecordSet
    Condition: IsBlueEnvironment
    Properties:
      HostedZoneId: !Ref HostedZone
      Name: !Sub 'app.${DomainName}'
      Type: A
      SetIdentifier: 'blue'
      Weight: !If [IsBlueEnvironment, 100, 0]
      AliasTarget:
        DNSName: !GetAtt ApplicationStack.Outputs.LoadBalancerDNS
        HostedZoneId: !GetAtt ApplicationStack.Outputs.LoadBalancerHostedZone

  GreenRecord:
    Type: AWS::Route53::RecordSet
    Condition: IsGreenEnvironment
    Properties:
      HostedZoneId: !Ref HostedZone
      Name: !Sub 'app.${DomainName}'
      Type: A
      SetIdentifier: 'green'
      Weight: !If [IsGreenEnvironment, 100, 0]
      AliasTarget:
        DNSName: !GetAtt ApplicationStack.Outputs.LoadBalancerDNS
        HostedZoneId: !GetAtt ApplicationStack.Outputs.LoadBalancerHostedZone

# Traffic switching script
TrafficSwitchFunction:
  Type: AWS::Lambda::Function
  Properties:
    Runtime: python3.9
    Handler: index.lambda_handler
    Code:
      ZipFile: |
        import boto3
        import json
        
        def lambda_handler(event, context):
            route53 = boto3.client('route53')
            
            # Get current traffic weights
            current_blue_weight = event.get('blue_weight', 0)
            current_green_weight = event.get('green_weight', 100)
            
            # Switch traffic gradually
            if event.get('action') == 'switch_to_green':
                new_blue_weight = max(0, current_blue_weight - 10)
                new_green_weight = min(100, current_green_weight + 10)
            elif event.get('action') == 'switch_to_blue':
                new_blue_weight = min(100, current_blue_weight + 10)
                new_green_weight = max(0, current_green_weight - 10)
            
            # Update Route 53 records
            # Implementation depends on your specific setup
            
            return {
                'statusCode': 200,
                'body': json.dumps({
                    'blue_weight': new_blue_weight,
                    'green_weight': new_green_weight
                })
            }
```

### **Change Set Deployment Script**

#### **Safe Deployment avec Preview**
```bash
#!/bin/bash
# deploy-with-changeset.sh

STACK_NAME=$1
TEMPLATE_FILE=$2
PARAMETERS_FILE=$3
ENVIRONMENT=$4

if [ -z "$STACK_NAME" ] || [ -z "$TEMPLATE_FILE" ]; then
    echo "Usage: $0 <stack-name> <template-file> [parameters-file] [environment]"
    exit 1
fi

echo "🚀 Starting deployment of $STACK_NAME..."

# Create change set
CHANGESET_NAME="changeset-$(date +%Y%m%d-%H%M%S)"

echo "📋 Creating change set: $CHANGESET_NAME"

if [ -n "$PARAMETERS_FILE" ]; then
    aws cloudformation create-change-set \
        --stack-name "$STACK_NAME" \
        --change-set-name "$CHANGESET_NAME" \
        --template-body file://"$TEMPLATE_FILE" \
        --parameters file://"$PARAMETERS_FILE" \
        --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM \
        --tags Key=Environment,Value="$ENVIRONMENT" \
               Key=DeployedBy,Value="$(whoami)" \
               Key=DeployedAt,Value="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
else
    aws cloudformation create-change-set \
        --stack-name "$STACK_NAME" \
        --change-set-name "$CHANGESET_NAME" \
        --template-body file://"$TEMPLATE_FILE" \
        --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM \
        --tags Key=Environment,Value="$ENVIRONMENT" \
               Key=DeployedBy,Value="$(whoami)" \
               Key=DeployedAt,Value="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
fi

# Wait for change set creation
echo "⏳ Waiting for change set creation..."
aws cloudformation wait change-set-create-complete \
    --stack-name "$STACK_NAME" \
    --change-set-name "$CHANGESET_NAME"

# Describe changes
echo "📊 Change set details:"
aws cloudformation describe-change-set \
    --stack-name "$STACK_NAME" \
    --change-set-name "$CHANGESET_NAME" \
    --query 'Changes[*].[Action,ResourceChange.LogicalResourceId,ResourceChange.ResourceType,ResourceChange.Replacement]' \
    --output table

# Ask for confirmation
echo ""
read -p "🤔 Do you want to execute this change set? (y/N): " confirm

if [[ $confirm == [yY] || $confirm == [yY][eE][sS] ]]; then
    echo "✅ Executing change set..."
    
    aws cloudformation execute-change-set \
        --stack-name "$STACK_NAME" \
        --change-set-name "$CHANGESET_NAME"
    
    echo "⏳ Waiting for stack update to complete..."
    aws cloudformation wait stack-update-complete \
        --stack-name "$STACK_NAME"
    
    if [ $? -eq 0 ]; then
        echo "🎉 Stack update completed successfully!"
        
        # Get stack outputs
        echo "📤 Stack outputs:"
        aws cloudformation describe-stacks \
            --stack-name "$STACK_NAME" \
            --query 'Stacks[0].Outputs[*].[OutputKey,OutputValue,Description]' \
            --output table
    else
        echo "❌ Stack update failed!"
        exit 1
    fi
else
    echo "❌ Change set execution cancelled"
    
    # Clean up change set
    aws cloudformation delete-change-set \
        --stack-name "$STACK_NAME" \
        --change-set-name "$CHANGESET_NAME"
    
    echo "🧹 Change set deleted"
fi
```

---

## ✅ **Quiz AWS CloudFormation**

### **Question 1:** Quelle est la différence entre un Change Set et un direct update dans CloudFormation ?
<details>
<summary>Réponse</summary>

**Change Set :**
- **Preview** des modifications avant exécution
- Permet de **réviser** les changements
- **Sécurisé** pour les environnements de production
- Peut être **supprimé** sans impact
- Montre les **ressources affectées** et le type de changement

**Direct Update :**
- Exécution **immédiate** des changements
- **Pas de preview** des modifications
- **Risqué** pour les environnements critiques
- **Rollback** uniquement après échec
- Plus **rapide** pour les environments de développement

**Best Practice :** Utiliser Change Sets pour production, direct updates pour développement.

```bash
# Change Set approach
aws cloudformation create-change-set --stack-name prod-stack --change-set-name review-changes
aws cloudformation describe-change-set --change-set-name review-changes
aws cloudformation execute-change-set --change-set-name review-changes

# Direct update
aws cloudformation update-stack --stack-name dev-stack --template-body file://template.yaml
```
</details>

### **Question 2:** Comment gérer les secrets dans CloudFormation templates ?
<details>
<summary>Réponse</summary>

**Methods pour gérer les secrets :**

1. **AWS Systems Manager Parameter Store :**
```yaml
DatabasePassword:
  Type: AWS::SSM::Parameter::Value<String>
  Default: /myapp/database/password
  NoEcho: true
```

2. **AWS Secrets Manager :**
```yaml
DatabaseSecret:
  Type: AWS::SecretsManager::Secret
  Properties:
    Description: Database password
    GenerateSecretString:
      SecretStringTemplate: '{"username": "admin"}'
      GenerateStringKey: 'password'
      PasswordLength: 32
      ExcludeCharacters: '"@/\'
```

3. **Dynamic References :**
```yaml
Environment:
  Variables:
    DB_PASSWORD: !Sub '{{resolve:secretsmanager:${DatabaseSecret}:SecretString:password}}'
    API_KEY: !Sub '{{resolve:ssm-secure:/myapp/api-key:1}}'
```

**❌ À éviter :**
- Hardcoder secrets dans templates
- Passer secrets via parameters
- Stocker secrets en plain text

**✅ Best Practices :**
- Utiliser dynamic references
- Rotation automatique des secrets
- Least privilege access
- Audit trail avec CloudTrail
</details>

### **Question 3:** Comment implémenter une stratégie de rollback avec CloudFormation ?
<details>
<summary>Réponse</summary>

**Stratégies de Rollback :**

1. **Automatic Rollback :**
```yaml
# Default behavior - rollback on failure
aws cloudformation update-stack \
  --stack-name my-stack \
  --template-body file://template.yaml \
  --disable-rollback false  # Default
```

2. **Manual Rollback :**
```bash
# Cancel update and rollback
aws cloudformation cancel-update-stack --stack-name my-stack

# Continue rollback after manual intervention
aws cloudformation continue-update-rollback --stack-name my-stack
```

3. **Blue/Green avec Route 53 :**
```yaml
# Gradual traffic shifting
BlueEnvironment:
  Weight: 90  # Reduce gradually
GreenEnvironment:
  Weight: 10  # Increase gradually
```

4. **Stack Policies pour protection :**
```json
{
  "Statement": [
    {
      "Effect": "Deny",
      "Action": "Update:Delete",
      "Principal": "*",
      "Resource": "LogicalResourceId/ProductionDatabase"
    }
  ]
}
```

**Monitoring pour Rollback :**
- CloudWatch Alarms
- Health checks
- Custom metrics
- Lambda-based validation
</details>

### **Question 4:** Comment organiser des templates CloudFormation pour un environnement multi-compte ?
<details>
<summary>Réponse</summary>

**Architecture Multi-Compte :**

1. **Nested Stacks :**
```yaml
# Master template
NetworkStack:
  Type: AWS::CloudFormation::Stack
  Properties:
    TemplateURL: s3://templates/network.yaml
    Parameters:
      Environment: !Ref Environment

ApplicationStack:
  Type: AWS::CloudFormation::Stack
  DependsOn: NetworkStack
  Properties:
    TemplateURL: s3://templates/application.yaml
    Parameters:
      NetworkStackName: !Ref NetworkStack
```

2. **Cross-Account References :**
```yaml
# Account A exports
Outputs:
  SharedVPC:
    Value: !Ref VPC
    Export:
      Name: !Sub 'shared-vpc-${Environment}'

# Account B imports
VpcId:
  Fn::ImportValue: !Sub 'shared-vpc-${Environment}'
```

3. **StackSets pour Multi-Account :**
```bash
# Deploy to multiple accounts
aws cloudformation create-stack-set \
  --stack-set-name security-baseline \
  --template-body file://security-template.yaml \
  --accounts 123456789012,234567890123 \
  --regions us-east-1,eu-west-1
```

**Structure Recommended :**
```
templates/
├── master/
│   ├── account-baseline.yaml
│   └── organization-units.yaml
├── shared/
│   ├── network.yaml
│   └── security.yaml
└── applications/
    ├── web-app.yaml
    └── api-service.yaml
```

**Best Practices :**
- Separate templates par responsabilité
- Use exports/imports pour cross-stack references
- StackSets pour governance et compliance
- Parameterization pour environnements
</details>

---

## 🎯 **Points Clés pour Certification AWS**

### **CloudFormation Core Features**
- **Templates** en YAML/JSON avec sections complètes
- **Stacks** pour grouper et gérer resources
- **Change Sets** pour preview et safe deployments
- **Stack Policies** pour protection resources critiques
- **Drift Detection** pour identifier changes manuels

### **Advanced Patterns**
- **Nested Stacks** pour modularité et réutilisation
- **Cross-Stack References** avec Exports/Imports
- **StackSets** pour déploiements multi-comptes/régions
- **Custom Resources** avec Lambda pour extensibilité
- **Macros** pour template transformation

### **DevOps Integration**
- **CI/CD pipelines** avec CodePipeline integration
- **Blue/Green deployments** avec Route 53 weighting
- **Rollback strategies** automatiques et manuelles
- **Monitoring** avec CloudWatch et SNS notifications
- **Security** avec IAM roles et secret management

---

**🎯 Next: AWS CDK - Modern Infrastructure as Code →**
