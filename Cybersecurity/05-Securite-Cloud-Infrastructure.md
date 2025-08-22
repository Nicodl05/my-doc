# **Sécurité Cloud & Infrastructure - Fiche de Révision Entretien**

## **Table des matières**
1. [Modèles de responsabilité Cloud](#modèles-de-responsabilité-cloud)
2. [Sécurité AWS](#sécurité-aws)
3. [Sécurité Azure](#sécurité-azure)
4. [Sécurité Multi-Cloud](#sécurité-multi-cloud)
5. [Identity and Access Management (IAM)](#identity-and-access-management-iam)
6. [Network Security](#network-security)
7. [Data Protection et Encryption](#data-protection-et-encryption)
8. [Monitoring et Compliance](#monitoring-et-compliance)
9. [Disaster Recovery et Business Continuity](#disaster-recovery-et-business-continuity)

---

## **Modèles de responsabilité Cloud**

### Shared Responsibility Model

#### 1. Infrastructure as a Service (IaaS)
```
Provider Responsibility:
├── Physical Security
├── Network Infrastructure
├── Hypervisor
└── Host Operating System

Customer Responsibility:
├── Guest Operating System
├── Applications
├── Data
├── Network Configuration
├── Firewall Configuration
└── Identity Management
```

#### 2. Platform as a Service (PaaS)
```
Provider Responsibility:
├── Physical Security
├── Network Infrastructure
├── Hypervisor
├── Host Operating System
├── Container Runtime
└── Platform Services

Customer Responsibility:
├── Applications
├── Data
├── User Access Management
└── Application-level Security
```

#### 3. Software as a Service (SaaS)
```
Provider Responsibility:
├── Physical Security
├── Network Infrastructure
├── Platform
├── Application
└── Most Security Controls

Customer Responsibility:
├── Data Classification
├── User Access Management
├── Endpoint Protection
└── Account Security
```

---

## **Sécurité AWS**

### AWS Security Services Architecture

```csharp
// AWS Security Configuration Service
public class AWSSecurityService
{
    private readonly IAmazonIdentityManagementService _iamService;
    private readonly IAmazonCloudWatch _cloudWatch;
    private readonly IAmazonGuardDuty _guardDuty;
    private readonly IAmazonSecurityHub _securityHub;
    
    public AWSSecurityService(
        IAmazonIdentityManagementService iamService,
        IAmazonCloudWatch cloudWatch,
        IAmazonGuardDuty guardDuty,
        IAmazonSecurityHub securityHub)
    {
        _iamService = iamService;
        _cloudWatch = cloudWatch;
        _guardDuty = guardDuty;
        _securityHub = securityHub;
    }
    
    public async Task<SecurityAssessmentResult> AssessSecurityPostureAsync()
    {
        var tasks = new List<Task>
        {
            CheckIAMPoliciesAsync(),
            CheckS3BucketSecurityAsync(), 
            CheckVPCSecurityGroupsAsync(),
            CheckCloudTrailConfigurationAsync(),
            CheckGuardDutyFindingsAsync()
        };
        
        await Task.WhenAll(tasks);
        
        return new SecurityAssessmentResult
        {
            OverallScore = CalculateSecurityScore(),
            Findings = await GetSecurityFindingsAsync(),
            Recommendations = GenerateRecommendations()
        };
    }
    
    private async Task CheckIAMPoliciesAsync()
    {
        // Vérifier les politiques IAM trop permissives
        var users = await _iamService.ListUsersAsync();
        
        foreach (var user in users.Users)
        {
            var policies = await _iamService.ListAttachedUserPoliciesAsync(
                new ListAttachedUserPoliciesRequest { UserName = user.UserName });
            
            foreach (var policy in policies.AttachedPolicies)
            {
                var policyVersion = await _iamService.GetPolicyVersionAsync(
                    new GetPolicyVersionRequest 
                    { 
                        PolicyArn = policy.PolicyArn,
                        VersionId = "v1"
                    });
                
                if (HasOverlyPermissivePolicy(policyVersion.PolicyVersion.Document))
                {
                    LogSecurityFinding($"User {user.UserName} has overly permissive policy {policy.PolicyName}");
                }
            }
        }
    }
}
```

### AWS CloudFormation Security Templates

#### 1. Secure VPC Configuration
```yaml
# templates/secure-vpc.yml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Secure VPC with multiple layers of security'

Parameters:
  Environment:
    Type: String
    AllowedValues: [dev, staging, prod]
    Default: dev

Resources:
  # VPC avec DNS sécurisé
  SecureVPC:
    Type: AWS::EC2::VPC
    Properties:
      CidrBlock: 10.0.0.0/16
      EnableDnsHostnames: true
      EnableDnsSupport: true
      Tags:
        - Key: Name
          Value: !Sub "${Environment}-secure-vpc"
        - Key: Environment
          Value: !Ref Environment

  # Subnets privés
  PrivateSubnet1:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref SecureVPC
      CidrBlock: 10.0.1.0/24
      AvailabilityZone: !Select [0, !GetAZs '']
      MapPublicIpOnLaunch: false
      Tags:
        - Key: Name
          Value: !Sub "${Environment}-private-subnet-1"

  PrivateSubnet2:
    Type: AWS::EC2::Subnet
    Properties:
      VpcId: !Ref SecureVPC
      CidrBlock: 10.0.2.0/24
      AvailabilityZone: !Select [1, !GetAZs '']
      MapPublicIpOnLaunch: false
      Tags:
        - Key: Name
          Value: !Sub "${Environment}-private-subnet-2"

  # Security Group restrictif
  WebSecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Security group for web servers
      VpcId: !Ref SecureVPC
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 443
          ToPort: 443
          CidrIp: 0.0.0.0/0
          Description: "HTTPS from anywhere"
        - IpProtocol: tcp
          FromPort: 80
          ToPort: 80
          CidrIp: 0.0.0.0/0
          Description: "HTTP from anywhere (redirect to HTTPS)"
      SecurityGroupEgress:
        - IpProtocol: tcp
          FromPort: 443
          ToPort: 443
          CidrIp: 0.0.0.0/0
          Description: "HTTPS outbound"
      Tags:
        - Key: Name
          Value: !Sub "${Environment}-web-sg"

  # WAF pour protection web
  WebACL:
    Type: AWS::WAFv2::WebACL
    Properties:
      Name: !Sub "${Environment}-web-acl"
      Scope: REGIONAL
      DefaultAction:
        Allow: {}
      Rules:
        - Name: AWSManagedRulesCommonRuleSet
          Priority: 1
          OverrideAction:
            None: {}
          Statement:
            ManagedRuleGroupStatement:
              VendorName: AWS
              Name: AWSManagedRulesCommonRuleSet
          VisibilityConfig:
            SampledRequestsEnabled: true
            CloudWatchMetricsEnabled: true
            MetricName: CommonRuleSetMetric
        - Name: RateLimitRule
          Priority: 2
          Action:
            Block: {}
          Statement:
            RateBasedStatement:
              Limit: 2000
              AggregateKeyType: IP
          VisibilityConfig:
            SampledRequestsEnabled: true
            CloudWatchMetricsEnabled: true
            MetricName: RateLimitMetric

  # CloudTrail pour l'audit
  CloudTrail:
    Type: AWS::CloudTrail::Trail
    Properties:
      TrailName: !Sub "${Environment}-security-trail"
      S3BucketName: !Ref CloudTrailBucket
      S3KeyPrefix: !Sub "${Environment}/cloudtrail/"
      IncludeGlobalServiceEvents: true
      IsMultiRegionTrail: true
      EnableLogFileValidation: true
      EventSelectors:
        - ReadWriteType: All
          IncludeManagementEvents: true
          DataResources:
            - Type: "AWS::S3::Object"
              Values: ["*"]
            - Type: "AWS::Lambda::Function"
              Values: ["*"]

  # Bucket S3 sécurisé pour les logs
  CloudTrailBucket:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Sub "${Environment}-cloudtrail-logs-${AWS::AccountId}"
      BucketEncryption:
        ServerSideEncryptionConfiguration:
          - ServerSideEncryptionByDefault:
              SSEAlgorithm: AES256
      PublicAccessBlockConfiguration:
        BlockPublicAcls: true
        BlockPublicPolicy: true
        IgnorePublicAcls: true
        RestrictPublicBuckets: true
      NotificationConfiguration:
        CloudWatchConfigurations:
          - Event: s3:ObjectCreated:*
            CloudWatchConfiguration:
              LogGroupName: !Ref CloudTrailLogGroup

  # GuardDuty pour la détection de menaces
  GuardDutyDetector:
    Type: AWS::GuardDuty::Detector
    Properties:
      Enable: true
      FindingPublishingFrequency: FIFTEEN_MINUTES
```

#### 2. Secure IAM Roles et Policies
```yaml
# templates/iam-security.yml
  # Rôle pour application avec permissions minimales
  ApplicationRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: !Sub "${Environment}-application-role"
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              Service: ec2.amazonaws.com
            Action: sts:AssumeRole
            Condition:
              StringEquals:
                'aws:RequestedRegion': !Ref AWS::Region
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy
      Policies:
        - PolicyName: ApplicationS3Access
          PolicyDocument:
            Version: '2012-10-17'
            Statement:
              - Effect: Allow
                Action:
                  - s3:GetObject
                  - s3:PutObject
                Resource: 
                  - !Sub "${ApplicationBucket}/*"
              - Effect: Allow
                Action:
                  - s3:ListBucket
                Resource: !Ref ApplicationBucket
                Condition:
                  StringLike:
                    's3:prefix': 
                      - !Sub "${Environment}/*"

  # Cross-Account Role avec conditions strictes
  CrossAccountRole:
    Type: AWS::IAM::Role
    Properties:
      RoleName: !Sub "${Environment}-cross-account-role"
      AssumeRolePolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Effect: Allow
            Principal:
              AWS: !Sub "arn:aws:iam::${TrustedAccountId}:root"
            Action: sts:AssumeRole
            Condition:
              StringEquals:
                'sts:ExternalId': !Ref ExternalId
              IpAddress:
                'aws:SourceIp': 
                  - "203.0.113.0/24"  # IP autorisées
              DateGreaterThan:
                'aws:CurrentTime': "2024-01-01T00:00:00Z"
              DateLessThan:
                'aws:CurrentTime': "2025-12-31T23:59:59Z"

  # Policy avec conditions de sécurité avancées
  SecureS3Policy:
    Type: AWS::IAM::Policy
    Properties:
      PolicyName: SecureS3Access
      PolicyDocument:
        Version: '2012-10-17'
        Statement:
          - Sid: AllowSSLRequestsOnly
            Effect: Deny
            Principal: "*"
            Action: "s3:*"
            Resource:
              - !Sub "${ApplicationBucket}/*"
              - !Ref ApplicationBucket
            Condition:
              Bool:
                'aws:SecureTransport': 'false'
          - Sid: RequireEncryption
            Effect: Deny
            Principal: "*"
            Action: "s3:PutObject"
            Resource: !Sub "${ApplicationBucket}/*"
            Condition:
              StringNotEquals:
                's3:x-amz-server-side-encryption': 'AES256'
      Roles:
        - !Ref ApplicationRole
```

### AWS Security Best Practices Implementation

```csharp
// AWS Security Hardening Service
public class AWSSecurityHardeningService
{
    private readonly IAmazonSecurityHub _securityHub;
    private readonly IAmazonConfigService _configService;
    
    public async Task HardenAWSEnvironmentAsync()
    {
        // 1. Enable AWS Config Rules
        await EnableSecurityConfigRulesAsync();
        
        // 2. Setup Security Hub
        await SetupSecurityHubAsync();
        
        // 3. Configure VPC Flow Logs
        await EnableVPCFlowLogsAsync();
        
        // 4. Setup CloudWatch Security Alarms
        await SetupSecurityAlarmsAsync();
        
        // 5. Enable GuardDuty
        await EnableGuardDutyAsync();
    }
    
    private async Task EnableSecurityConfigRulesAsync()
    {
        var securityRules = new[]
        {
            "s3-bucket-public-access-prohibited",
            "s3-bucket-ssl-requests-only", 
            "iam-policy-no-statements-with-admin-access",
            "ec2-security-group-attached-to-eni",
            "rds-storage-encrypted",
            "cloudtrail-enabled"
        };
        
        foreach (var ruleName in securityRules)
        {
            await _configService.PutConfigRuleAsync(new PutConfigRuleRequest
            {
                ConfigRule = new ConfigRule
                {
                    ConfigRuleName = ruleName,
                    Source = new Source
                    {
                        Owner = "AWS",
                        SourceIdentifier = ruleName
                    }
                }
            });
        }
    }
    
    private async Task SetupSecurityAlarmsAsync()
    {
        var cloudWatch = new AmazonCloudWatchClient();
        
        // Alarme pour les échecs de connexion
        await cloudWatch.PutMetricAlarmAsync(new PutMetricAlarmRequest
        {
            AlarmName = "HighAuthenticationFailures",
            AlarmDescription = "Alert when authentication failures are high",
            MetricName = "SigninFailures",
            Namespace = "AWS/Events",
            Statistic = "Sum",
            Period = 300,
            EvaluationPeriods = 2,
            Threshold = 10,
            ComparisonOperator = ComparisonOperator.GreaterThanThreshold,
            AlarmActions = new List<string> { "arn:aws:sns:region:account:security-alerts" }
        });
        
        // Alarme pour l'utilisation de privilèges root
        await cloudWatch.PutMetricAlarmAsync(new PutMetricAlarmRequest
        {
            AlarmName = "RootAccountUsage",
            AlarmDescription = "Alert when root account is used",
            MetricName = "RootAccountUsageEventCount",
            Namespace = "CloudWatchLogMetrics",
            Statistic = "Sum",
            Period = 300,
            EvaluationPeriods = 1,
            Threshold = 1,
            ComparisonOperator = ComparisonOperator.GreaterThanOrEqualToThreshold
        });
    }
}
```

---

## **Sécurité Azure**

### Azure Security Architecture

```csharp
// Azure Security Service
public class AzureSecurityService
{
    private readonly TokenCredential _credential;
    private readonly ArmClient _armClient;
    
    public AzureSecurityService(TokenCredential credential)
    {
        _credential = credential;
        _armClient = new ArmClient(credential);
    }
    
    public async Task<AzureSecurityAssessment> AssessAzureSecurityAsync(string subscriptionId)
    {
        var subscription = _armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscriptionId));
        
        var assessment = new AzureSecurityAssessment();
        
        // 1. Évaluer Azure Security Center
        assessment.SecurityCenterFindings = await GetSecurityCenterFindingsAsync(subscription);
        
        // 2. Vérifier les Network Security Groups
        assessment.NetworkSecurityIssues = await CheckNetworkSecurityGroupsAsync(subscription);
        
        // 3. Analyser les configurations Key Vault
        assessment.KeyVaultIssues = await AnalyzeKeyVaultConfigurationsAsync(subscription);
        
        // 4. Vérifier l'identité et l'accès
        assessment.IdentityIssues = await CheckIdentityConfigurationAsync(subscription);
        
        // 5. Analyser les configurations de stockage
        assessment.StorageIssues = await CheckStorageSecurityAsync(subscription);
        
        return assessment;
    }
    
    private async Task<List<SecurityFinding>> CheckNetworkSecurityGroupsAsync(
        SubscriptionResource subscription)
    {
        var findings = new List<SecurityFinding>();
        
        await foreach (var resourceGroup in subscription.GetResourceGroups())
        {
            await foreach (var nsg in resourceGroup.GetNetworkSecurityGroups())
            {
                var nsgData = await nsg.GetAsync();
                
                foreach (var rule in nsgData.Value.Data.SecurityRules)
                {
                    // Vérifier les règles dangereuses
                    if (rule.Access == SecurityRuleAccess.Allow &&
                        rule.Direction == SecurityRuleDirection.Inbound &&
                        rule.SourceAddressPrefix == "*" &&
                        IsDangerousPort(rule.DestinationPortRange))
                    {
                        findings.Add(new SecurityFinding
                        {
                            Severity = FindingSeverity.High,
                            Type = "Overly Permissive NSG Rule",
                            Resource = nsg.Id,
                            Description = $"NSG rule allows access to {rule.DestinationPortRange} from any source",
                            Recommendation = "Restrict source IP ranges to specific networks"
                        });
                    }
                }
            }
        }
        
        return findings;
    }
}
```

### Azure Resource Manager Templates Security

#### 1. Secure Network Configuration
```json
{
    "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "environment": {
            "type": "string",
            "allowedValues": ["dev", "staging", "prod"],
            "defaultValue": "dev"
        }
    },
    "variables": {
        "vnetName": "[concat(parameters('environment'), '-secure-vnet')]",
        "nsgName": "[concat(parameters('environment'), '-web-nsg')]"
    },
    "resources": [
        {
            "type": "Microsoft.Network/networkSecurityGroups",
            "apiVersion": "2021-02-01",
            "name": "[variables('nsgName')]",
            "location": "[resourceGroup().location]",
            "properties": {
                "securityRules": [
                    {
                        "name": "AllowHTTPS",
                        "properties": {
                            "priority": 1000,
                            "access": "Allow",
                            "direction": "Inbound",
                            "protocol": "Tcp",
                            "sourcePortRange": "*",
                            "destinationPortRange": "443",
                            "sourceAddressPrefix": "*",
                            "destinationAddressPrefix": "*"
                        }
                    },
                    {
                        "name": "AllowHTTP",
                        "properties": {
                            "priority": 1001,
                            "access": "Allow",
                            "direction": "Inbound",
                            "protocol": "Tcp",
                            "sourcePortRange": "*",
                            "destinationPortRange": "80",
                            "sourceAddressPrefix": "*",
                            "destinationAddressPrefix": "*"
                        }
                    },
                    {
                        "name": "DenyAllInbound",
                        "properties": {
                            "priority": 4096,
                            "access": "Deny",
                            "direction": "Inbound",
                            "protocol": "*",
                            "sourcePortRange": "*",
                            "destinationPortRange": "*",
                            "sourceAddressPrefix": "*",
                            "destinationAddressPrefix": "*"
                        }
                    }
                ]
            }
        },
        {
            "type": "Microsoft.KeyVault/vaults",
            "apiVersion": "2021-04-01-preview",
            "name": "[concat(parameters('environment'), '-kv-', uniqueString(resourceGroup().id))]",
            "location": "[resourceGroup().location]",
            "properties": {
                "sku": {
                    "family": "A",
                    "name": "premium"
                },
                "tenantId": "[subscription().tenantId]",
                "enabledForDeployment": false,
                "enabledForDiskEncryption": true,
                "enabledForTemplateDeployment": false,
                "enableSoftDelete": true,
                "softDeleteRetentionInDays": 90,
                "enablePurgeProtection": true,
                "networkAcls": {
                    "defaultAction": "Deny",
                    "bypass": "AzureServices",
                    "ipRules": [],
                    "virtualNetworkRules": [
                        {
                            "id": "[concat(resourceId('Microsoft.Network/virtualNetworks', variables('vnetName')), '/subnets/default')]"
                        }
                    ]
                },
                "accessPolicies": []
            }
        }
    ]
}
```

### Azure Security Center Integration

```csharp
// Azure Security Center Monitoring
public class AzureSecurityCenterService
{
    private readonly SecurityCenterClient _securityClient;
    
    public async Task<SecurityAssessmentSummary> GetSecurityAssessmentAsync()
    {
        var assessments = await _securityClient.Assessments.ListAsync();
        var recommendations = await _securityClient.Tasks.ListAsync();
        var alerts = await _securityClient.Alerts.ListAsync();
        
        return new SecurityAssessmentSummary
        {
            OverallSecureScore = await CalculateSecureScoreAsync(),
            HighSeverityFindings = assessments.Where(a => a.Status.Severity == "High").Count(),
            OpenRecommendations = recommendations.Count(),
            SecurityAlerts = alerts.Where(a => a.Status == "Active").Count(),
            ComplianceScore = await GetComplianceScoreAsync()
        };
    }
    
    public async Task EnableSecurityPoliciesAsync()
    {
        var policies = new[]
        {
            "Enable auditing on SQL servers",
            "Require encryption in transit",
            "Enable network security groups", 
            "Enable just-in-time network access",
            "Enable adaptive application controls"
        };
        
        foreach (var policy in policies)
        {
            await EnableSecurityPolicyAsync(policy);
        }
    }
    
    private async Task SetupCustomSecurityPolicyAsync()
    {
        var policyDefinition = new
        {
            properties = new
            {
                displayName = "Custom Security Requirements",
                policyType = "Custom",
                mode = "All",
                description = "Enforce organizational security requirements",
                policyRule = new
                {
                    @if = new
                    {
                        allOf = new[]
                        {
                            new { field = "type", equals = "Microsoft.Storage/storageAccounts" }
                        }
                    },
                    then = new
                    {
                        effect = "deny",
                        details = new
                        {
                            type = "Microsoft.Storage/storageAccounts",
                            name = "[field('name')]",
                            evaluationDelay = "AfterProvisioning",
                            existenceCondition = new
                            {
                                field = "Microsoft.Storage/storageAccounts/encryption.services.blob.enabled",
                                equals = "true"
                            }
                        }
                    }
                }
            }
        };
        
        // Appliquer la politique personnalisée
        await ApplyCustomPolicyAsync(policyDefinition);
    }
}
```

---

## **Identity and Access Management (IAM)**

### Zero Trust Identity Framework

```csharp
// Zero Trust Identity Service
public class ZeroTrustIdentityService
{
    private readonly IConditionalAccessService _conditionalAccess;
    private readonly IRiskAssessmentService _riskAssessment;
    private readonly IPrivilegedAccessService _privilegedAccess;
    
    public async Task<AccessDecision> EvaluateAccessRequestAsync(AccessRequest request)
    {
        // 1. Vérification d'identité forte
        var identityVerification = await VerifyIdentityAsync(request.User);
        if (!identityVerification.IsVerified)
        {
            return AccessDecision.Deny("Identity verification failed");
        }
        
        // 2. Évaluation des risques contextuels
        var riskAssessment = await _riskAssessment.AssessRiskAsync(new RiskContext
        {
            User = request.User,
            Device = request.Device,
            Location = request.Location,
            Resource = request.Resource,
            Time = request.Timestamp
        });
        
        // 3. Vérification des conditions d'accès
        var conditionalAccessResult = await _conditionalAccess.EvaluateAsync(request, riskAssessment);
        
        // 4. Contrôle d'accès dynamique
        if (riskAssessment.RiskLevel == RiskLevel.High)
        {
            return AccessDecision.RequireStepUp("High risk detected - additional authentication required");
        }
        
        if (request.IsPrivilegedAccess)
        {
            return await EvaluatePrivilegedAccessAsync(request, riskAssessment);
        }
        
        return conditionalAccessResult.Allow 
            ? AccessDecision.Allow() 
            : AccessDecision.Deny(conditionalAccessResult.Reason);
    }
    
    private async Task<AccessDecision> EvaluatePrivilegedAccessAsync(
        AccessRequest request, RiskAssessment riskAssessment)
    {
        // Just-In-Time (JIT) Access
        var jitAccess = await _privilegedAccess.RequestJITAccessAsync(new JITRequest
        {
            User = request.User,
            Resource = request.Resource,
            Duration = TimeSpan.FromHours(1),
            Justification = request.Justification,
            ApprovalRequired = riskAssessment.RiskLevel >= RiskLevel.Medium
        });
        
        if (!jitAccess.IsApproved)
        {
            return AccessDecision.RequireApproval("Privileged access requires approval");
        }
        
        // Privileged Access Workstation (PAW) requirement
        if (!request.Device.IsPAW)
        {
            return AccessDecision.Deny("Privileged access requires PAW device");
        }
        
        return AccessDecision.AllowWithMonitoring(jitAccess.SessionId);
    }
}

// Multi-Factor Authentication Service
public class MFAService
{
    public async Task<MFAChallenge> InitiateMFAChallengeAsync(string userId, MFAMethod method)
    {
        var user = await GetUserAsync(userId);
        
        return method switch
        {
            MFAMethod.SMS => await SendSMSChallengeAsync(user.PhoneNumber),
            MFAMethod.Email => await SendEmailChallengeAsync(user.Email),
            MFAMethod.TOTP => await GenerateTOTPChallengeAsync(user.TOTPSecret),
            MFAMethod.Push => await SendPushNotificationAsync(user.DeviceTokens),
            MFAMethod.FIDO2 => await InitiateFIDO2ChallengeAsync(user.FIDO2Credentials),
            _ => throw new NotSupportedException($"MFA method {method} not supported")
        };
    }
    
    public async Task<bool> ValidateMFAResponseAsync(string challengeId, string response)
    {
        var challenge = await GetChallengeAsync(challengeId);
        
        if (challenge.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        
        return challenge.Method switch
        {
            MFAMethod.SMS or MFAMethod.Email => ValidateOTPCode(challenge.ExpectedResponse, response),
            MFAMethod.TOTP => ValidateTOTPCode(challenge.Secret, response),
            MFAMethod.Push => challenge.ExpectedResponse == response,
            MFAMethod.FIDO2 => await ValidateFIDO2ResponseAsync(challenge.PublicKey, response),
            _ => false
        };
    }
}
```

### Role-Based Access Control (RBAC) Advanced

```csharp
// Advanced RBAC System
public class AdvancedRBACService
{
    public class Permission
    {
        public string Resource { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> Conditions { get; set; } = new();
    }
    
    public class Role
    {
        public string Name { get; set; }
        public List<Permission> Permissions { get; set; } = new();
        public TimeSpan MaxSessionDuration { get; set; }
        public bool RequiresApproval { get; set; }
    }
    
    public async Task<bool> HasPermissionAsync(string userId, string resource, string action, 
        Dictionary<string, object> context = null)
    {
        var user = await GetUserAsync(userId);
        var userRoles = await GetUserRolesAsync(userId);
        
        foreach (var roleName in userRoles)
        {
            var role = await GetRoleAsync(roleName);
            
            foreach (var permission in role.Permissions)
            {
                if (permission.Resource == resource && permission.Action == action)
                {
                    // Évaluer les conditions contextuelles
                    if (await EvaluateConditionsAsync(permission.Conditions, context))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    private async Task<bool> EvaluateConditionsAsync(
        Dictionary<string, object> conditions, 
        Dictionary<string, object> context)
    {
        foreach (var condition in conditions)
        {
            switch (condition.Key)
            {
                case "time_range":
                    if (!IsWithinTimeRange(condition.Value, DateTime.UtcNow))
                        return false;
                    break;
                    
                case "ip_range":
                    if (context?.ContainsKey("client_ip") == true)
                    {
                        if (!IsWithinIPRange(condition.Value.ToString(), context["client_ip"].ToString()))
                            return false;
                    }
                    break;
                    
                case "device_type":
                    if (context?.ContainsKey("device_type") == true)
                    {
                        if (!condition.Value.ToString().Equals(context["device_type"].ToString(), 
                            StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    break;
                    
                case "data_classification":
                    if (context?.ContainsKey("data_classification") == true)
                    {
                        var requiredLevel = (DataClassification)condition.Value;
                        var resourceLevel = (DataClassification)context["data_classification"];
                        if (resourceLevel > requiredLevel)
                            return false;
                    }
                    break;
            }
        }
        
        return true;
    }
}
```

---

## **Network Security**

### Cloud Network Security Architecture

```csharp
// Network Security Service
public class CloudNetworkSecurityService
{
    public async Task<NetworkSecurityConfig> DesignSecureNetworkAsync(
        NetworkRequirements requirements)
    {
        var config = new NetworkSecurityConfig();
        
        // 1. Segmentation réseau par zones de sécurité
        config.SecurityZones = DesignSecurityZones(requirements);
        
        // 2. Configuration des firewalls
        config.FirewallRules = await GenerateFirewallRulesAsync(config.SecurityZones);
        
        // 3. Configuration VPN et accès distant
        config.VPNConfiguration = ConfigureSecureVPN(requirements);
        
        // 4. Détection et prévention d'intrusion
        config.IDSConfiguration = ConfigureIntrustionDetection();
        
        // 5. Monitoring réseau
        config.NetworkMonitoring = ConfigureNetworkMonitoring();
        
        return config;
    }
    
    private List<SecurityZone> DesignSecurityZones(NetworkRequirements requirements)
    {
        return new List<SecurityZone>
        {
            new SecurityZone
            {
                Name = "DMZ",
                Description = "Demilitarized zone for public-facing services",
                SecurityLevel = SecurityLevel.Medium,
                AllowedTraffic = new[] { "HTTPS", "HTTP", "DNS" },
                SubnetCIDR = "10.0.1.0/24"
            },
            new SecurityZone
            {
                Name = "Web Tier",
                Description = "Web application servers",
                SecurityLevel = SecurityLevel.High,
                AllowedTraffic = new[] { "HTTPS from DMZ", "Database connections" },
                SubnetCIDR = "10.0.2.0/24"
            },
            new SecurityZone
            {
                Name = "Application Tier", 
                Description = "Business logic servers",
                SecurityLevel = SecurityLevel.High,
                AllowedTraffic = new[] { "Application protocols", "Database connections" },
                SubnetCIDR = "10.0.3.0/24"
            },
            new SecurityZone
            {
                Name = "Database Tier",
                Description = "Database servers",
                SecurityLevel = SecurityLevel.Critical,
                AllowedTraffic = new[] { "Database protocols from App Tier only" },
                SubnetCIDR = "10.0.4.0/24"
            },
            new SecurityZone
            {
                Name = "Management",
                Description = "Administrative and monitoring systems",
                SecurityLevel = SecurityLevel.Critical,
                AllowedTraffic = new[] { "SSH", "RDP", "SNMP", "Monitoring protocols" },
                SubnetCIDR = "10.0.10.0/24"
            }
        };
    }
    
    private async Task<List<FirewallRule>> GenerateFirewallRulesAsync(List<SecurityZone> zones)
    {
        var rules = new List<FirewallRule>();
        
        // Règles inter-zones
        rules.Add(new FirewallRule
        {
            Name = "DMZ to Web Tier",
            Source = "10.0.1.0/24",
            Destination = "10.0.2.0/24", 
            Protocol = "TCP",
            Ports = new[] { 443, 80 },
            Action = FirewallAction.Allow,
            Logging = true
        });
        
        rules.Add(new FirewallRule
        {
            Name = "Web to App Tier",
            Source = "10.0.2.0/24",
            Destination = "10.0.3.0/24",
            Protocol = "TCP", 
            Ports = new[] { 8080, 8443 },
            Action = FirewallAction.Allow,
            Logging = true
        });
        
        rules.Add(new FirewallRule
        {
            Name = "App to Database",
            Source = "10.0.3.0/24",
            Destination = "10.0.4.0/24",
            Protocol = "TCP",
            Ports = new[] { 5432, 1433 },
            Action = FirewallAction.Allow,
            Logging = true
        });
        
        // Règle de déni par défaut
        rules.Add(new FirewallRule
        {
            Name = "Default Deny All",
            Source = "any",
            Destination = "any",
            Protocol = "any",
            Ports = new[] { -1 },
            Action = FirewallAction.Deny,
            Logging = true,
            Priority = 65534
        });
        
        return rules;
    }
}
```

### Web Application Firewall (WAF) Configuration

```yaml
# WAF Rules Configuration
WAFConfiguration:
  name: "production-waf"
  mode: "Prevention"
  
  # OWASP Top 10 Protection
  owasp_rules:
    - rule_set: "OWASP_CRS_3.3"
      anomaly_threshold: 5
      paranoia_level: 2
      
  # Custom Rules
  custom_rules:
    - name: "Block SQL Injection"
      priority: 100
      conditions:
        - match_type: "Contains"
          match_values: ["'", "UNION", "SELECT", "DROP", "INSERT"]
          transforms: ["Lowercase", "RemoveNulls"]
      action: "Block"
      
    - name: "Rate Limiting"
      priority: 200
      conditions:
        - match_type: "IPAddress"
          operator: "Any"
      action: "RateLimit"
      rate_limit:
        requests_per_minute: 300
        duration_minutes: 5
        
    - name: "Geo-blocking"
      priority: 300
      conditions:
        - match_type: "GeoLocation"
          operator: "Contains"
          match_values: ["CN", "RU", "KP"]  # Block specific countries
      action: "Block"
      
    - name: "Bot Protection"
      priority: 400
      conditions:
        - match_type: "RequestHeader"
          selector: "User-Agent"
          operator: "Contains"
          match_values: ["bot", "crawler", "spider"]
      action: "Block"
      
  # Allowlist for trusted IPs
  ip_allowlist:
    - "203.0.113.0/24"    # Office network
    - "198.51.100.0/24"   # Partner network
    
  # Logging configuration
  logging:
    enabled: true
    include_request_body: true
    include_response_body: false
    retention_days: 90
```

---

## **Data Protection et Encryption**

### Cloud Data Encryption Strategy

```csharp
// Cloud Data Encryption Service
public class CloudDataEncryptionService
{
    private readonly IKeyManagementService _keyManagement;
    private readonly ICloudStorage _cloudStorage;
    
    public async Task<EncryptedData> EncryptDataAsync(byte[] data, DataClassification classification)
    {
        // Choisir la stratégie de chiffrement basée sur la classification
        var encryptionStrategy = GetEncryptionStrategy(classification);
        
        // Générer ou récupérer la clé de chiffrement
        var encryptionKey = await GetEncryptionKeyAsync(classification);
        
        // Chiffrer les données
        var encryptedData = await encryptionStrategy.EncryptAsync(data, encryptionKey);
        
        // Chiffrer la clé de données avec la clé maître
        var encryptedKey = await _keyManagement.EncryptKeyAsync(encryptionKey.KeyBytes);
        
        return new EncryptedData
        {
            EncryptedContent = encryptedData,
            EncryptedKey = encryptedKey,
            KeyId = encryptionKey.KeyId,
            Algorithm = encryptionStrategy.Algorithm,
            Classification = classification,
            EncryptedAt = DateTime.UtcNow
        };
    }
    
    private IEncryptionStrategy GetEncryptionStrategy(DataClassification classification)
    {
        return classification switch
        {
            DataClassification.Public => new AES128EncryptionStrategy(),
            DataClassification.Internal => new AES256EncryptionStrategy(),
            DataClassification.Confidential => new AES256GCMEncryptionStrategy(),
            DataClassification.Restricted => new ChaCha20Poly1305EncryptionStrategy(),
            _ => throw new ArgumentException($"Unknown classification: {classification}")
        };
    }
    
    public async Task SetupCustomerManagedKeysAsync()
    {
        var keyConfiguration = new KeyConfiguration
        {
            KeyType = KeyType.RSA,
            KeySize = 4096,
            KeyUsage = KeyUsage.Encrypt | KeyUsage.Decrypt,
            RotationPolicy = new KeyRotationPolicy
            {
                AutoRotationEnabled = true,
                RotationFrequency = TimeSpan.FromDays(90),
                NotificationDays = 7
            },
            AccessPolicy = new KeyAccessPolicy
            {
                AllowedOperations = new[] { "encrypt", "decrypt", "wrap", "unwrap" },
                AllowedPrincipals = new[] { "service-principal-id" },
                IPRestrictions = new[] { "203.0.113.0/24" },
                TimeRestrictions = new TimeRestriction
                {
                    StartTime = TimeSpan.FromHours(6),
                    EndTime = TimeSpan.FromHours(22),
                    TimeZone = "UTC"
                }
            }
        };
        
        await _keyManagement.CreateKeyAsync(keyConfiguration);
    }
}

// Database Encryption Service
public class DatabaseEncryptionService
{
    public async Task EnableTransparentDataEncryptionAsync(string databaseId)
    {
        // Configuration TDE avec clés gérées par le client
        var tdeConfig = new TransparentDataEncryptionConfig
        {
            DatabaseId = databaseId,
            EncryptionEnabled = true,
            KeyManagementType = KeyManagementType.CustomerManaged,
            KeyVaultKeyId = await GetDatabaseEncryptionKeyAsync(databaseId),
            RotationEnabled = true,
            RotationFrequency = TimeSpan.FromDays(365)
        };
        
        await ApplyTDEConfigurationAsync(tdeConfig);
    }
    
    public async Task ConfigureColumnLevelEncryptionAsync()
    {
        var encryptionColumns = new[]
        {
            new ColumnEncryption
            {
                TableName = "Users",
                ColumnName = "SSN",
                EncryptionType = ColumnEncryptionType.Deterministic,
                KeyName = "ssn-encryption-key"
            },
            new ColumnEncryption
            {
                TableName = "Users", 
                ColumnName = "CreditCardNumber",
                EncryptionType = ColumnEncryptionType.Randomized,
                KeyName = "pci-encryption-key"
            },
            new ColumnEncryption
            {
                TableName = "MedicalRecords",
                ColumnName = "Diagnosis",
                EncryptionType = ColumnEncryptionType.Randomized,
                KeyName = "hipaa-encryption-key"
            }
        };
        
        foreach (var column in encryptionColumns)
        {
            await EnableColumnEncryptionAsync(column);
        }
    }
}
```

### Backup and Recovery Security

```csharp
// Secure Backup Service
public class SecureBackupService
{
    public async Task<BackupJob> CreateSecureBackupAsync(BackupRequest request)
    {
        // 1. Validation des permissions
        await ValidateBackupPermissionsAsync(request.UserId, request.ResourceId);
        
        // 2. Chiffrement des données avant backup
        var encryptionKey = await GenerateBackupEncryptionKeyAsync();
        
        // 3. Configuration du backup sécurisé
        var backupConfig = new SecureBackupConfiguration
        {
            EncryptionEnabled = true,
            EncryptionAlgorithm = "AES-256-GCM",
            EncryptionKey = encryptionKey,
            CompressionEnabled = true,
            IntegrityCheckEnabled = true,
            ImmutableBackup = true, // Protection contre ransomware
            RetentionPolicy = new RetentionPolicy
            {
                DailyRetention = 30,
                WeeklyRetention = 12,
                MonthlyRetention = 12,
                YearlyRetention = 7
            },
            GeographicReplication = new GeographicReplication
            {
                Enabled = true,
                SecondaryRegion = GetSecondaryRegion(request.PrimaryRegion),
                CrossRegionEncryption = true
            }
        };
        
        // 4. Démarrer le backup avec monitoring
        var backupJob = await StartBackupJobAsync(request, backupConfig);
        
        // 5. Notifier le démarrage
        await NotifyBackupStartedAsync(backupJob);
        
        return backupJob;
    }
    
    public async Task<RestoreResult> PerformSecureRestoreAsync(RestoreRequest request)
    {
        // 1. Validation stricte des autorisations
        await ValidateRestorePermissionsAsync(request.UserId, request.BackupId);
        
        // 2. Vérification de l'intégrité du backup
        var integrityCheck = await VerifyBackupIntegrityAsync(request.BackupId);
        if (!integrityCheck.IsValid)
        {
            throw new BackupCorruptedException("Backup integrity check failed");
        }
        
        // 3. Déchiffrement sécurisé
        var decryptionKey = await GetBackupDecryptionKeyAsync(request.BackupId);
        
        // 4. Restauration avec audit
        var restoreResult = await RestoreWithAuditAsync(request, decryptionKey);
        
        // 5. Validation post-restauration
        await ValidateRestoredDataAsync(restoreResult);
        
        return restoreResult;
    }
}
```

---

## **Monitoring et Compliance**

### Security Information and Event Management (SIEM)

```csharp
// Cloud SIEM Service
public class CloudSIEMService
{
    private readonly IEventCollector _eventCollector;
    private readonly IThreatIntelligence _threatIntel;
    private readonly ISecurityAnalytics _analytics;
    
    public async Task ProcessSecurityEventAsync(SecurityEvent securityEvent)
    {
        // 1. Enrichissement des événements
        var enrichedEvent = await EnrichEventAsync(securityEvent);
        
        // 2. Corrélation avec l'intelligence des menaces
        var threatContext = await _threatIntel.GetThreatContextAsync(enrichedEvent);
        
        // 3. Analyse comportementale
        var behaviorAnalysis = await _analytics.AnalyzeBehaviorAsync(enrichedEvent);
        
        // 4. Scoring de risque
        var riskScore = CalculateRiskScore(enrichedEvent, threatContext, behaviorAnalysis);
        
        // 5. Alerting basé sur le score
        if (riskScore >= SecurityRiskThreshold.High)
        {
            await TriggerSecurityAlertAsync(enrichedEvent, riskScore);
        }
        
        // 6. Stockage pour investigation
        await StoreEventForInvestigationAsync(enrichedEvent, riskScore);
    }
    
    private async Task<EnrichedSecurityEvent> EnrichEventAsync(SecurityEvent originalEvent)
    {
        var enrichedEvent = new EnrichedSecurityEvent(originalEvent);
        
        // Enrichissement géographique
        if (!string.IsNullOrEmpty(originalEvent.SourceIP))
        {
            enrichedEvent.GeoLocation = await GetGeoLocationAsync(originalEvent.SourceIP);
            enrichedEvent.IsKnownMaliciousIP = await CheckMaliciousIPAsync(originalEvent.SourceIP);
        }
        
        // Enrichissement utilisateur
        if (!string.IsNullOrEmpty(originalEvent.UserId))
        {
            enrichedEvent.UserProfile = await GetUserProfileAsync(originalEvent.UserId);
            enrichedEvent.UserRiskProfile = await GetUserRiskProfileAsync(originalEvent.UserId);
        }
        
        // Enrichissement d'asset
        if (!string.IsNullOrEmpty(originalEvent.AssetId))
        {
            enrichedEvent.AssetProfile = await GetAssetProfileAsync(originalEvent.AssetId);
            enrichedEvent.AssetCriticality = await GetAssetCriticalityAsync(originalEvent.AssetId);
        }
        
        return enrichedEvent;
    }
    
    public async Task<List<SecurityIncident>> CorrelateEventsAsync(TimeSpan timeWindow)
    {
        var incidents = new List<SecurityIncident>();
        var events = await GetRecentEventsAsync(timeWindow);
        
        // Détection de patterns d'attaque
        
        // 1. Brute force attack
        var bruteForceAttacks = DetectBruteForceAttacks(events);
        incidents.AddRange(bruteForceAttacks);
        
        // 2. Lateral movement
        var lateralMovement = DetectLateralMovement(events);
        incidents.AddRange(lateralMovement);
        
        // 3. Data exfiltration
        var dataExfiltration = DetectDataExfiltration(events);
        incidents.AddRange(dataExfiltration);
        
        // 4. Privilege escalation
        var privilegeEscalation = DetectPrivilegeEscalation(events);
        incidents.AddRange(privilegeEscalation);
        
        // 5. Anomalous access patterns
        var anomalousAccess = DetectAnomalousAccessPatterns(events);
        incidents.AddRange(anomalousAccess);
        
        return incidents;
    }
}
```

### Compliance Automation

```csharp
// Compliance Automation Service
public class ComplianceAutomationService
{
    public async Task<ComplianceReport> GenerateComplianceReportAsync(
        ComplianceFramework framework, 
        DateTime reportPeriodStart,
        DateTime reportPeriodEnd)
    {
        var report = new ComplianceReport
        {
            Framework = framework,
            PeriodStart = reportPeriodStart,
            PeriodEnd = reportPeriodEnd,
            GeneratedAt = DateTime.UtcNow
        };
        
        switch (framework)
        {
            case ComplianceFramework.SOC2:
                report.Controls = await AssessSOC2ControlsAsync(reportPeriodStart, reportPeriodEnd);
                break;
                
            case ComplianceFramework.ISO27001:
                report.Controls = await AssessISO27001ControlsAsync(reportPeriodStart, reportPeriodEnd);
                break;
                
            case ComplianceFramework.PCIDSS:
                report.Controls = await AssessPCIDSSControlsAsync(reportPeriodStart, reportPeriodEnd);
                break;
                
            case ComplianceFramework.HIPAA:
                report.Controls = await AssessHIPAAControlsAsync(reportPeriodStart, reportPeriodEnd);
                break;
                
            case ComplianceFramework.GDPR:
                report.Controls = await AssessGDPRControlsAsync(reportPeriodStart, reportPeriodEnd);
                break;
        }
        
        report.OverallCompliance = CalculateOverallCompliance(report.Controls);
        report.Recommendations = GenerateRecommendations(report.Controls);
        
        return report;
    }
    
    private async Task<List<ControlAssessment>> AssessSOC2ControlsAsync(
        DateTime periodStart, DateTime periodEnd)
    {
        var assessments = new List<ControlAssessment>();
        
        // CC6.1 - Logical and physical access controls
        assessments.Add(await AssessControlAsync("CC6.1", async () =>
        {
            var accessEvents = await GetAccessEventsAsync(periodStart, periodEnd);
            var unauthorizedAccess = accessEvents.Where(e => !e.IsAuthorized).Count();
            return unauthorizedAccess == 0;
        }));
        
        // CC6.2 - Transmission of data
        assessments.Add(await AssessControlAsync("CC6.2", async () =>
        {
            var transmissions = await GetDataTransmissionsAsync(periodStart, periodEnd);
            var unencryptedTransmissions = transmissions.Where(t => !t.IsEncrypted).Count();
            return unencryptedTransmissions == 0;
        }));
        
        // CC6.3 - Encryption of data at rest
        assessments.Add(await AssessControlAsync("CC6.3", async () =>
        {
            var dataStores = await GetDataStoresAsync();
            var unencryptedStores = dataStores.Where(ds => !ds.IsEncrypted).Count();
            return unencryptedStores == 0;
        }));
        
        // CC7.1 - System boundaries and data classification
        assessments.Add(await AssessControlAsync("CC7.1", async () =>
        {
            var dataClassification = await GetDataClassificationStatusAsync();
            return dataClassification.CompliancePercentage >= 95;
        }));
        
        return assessments;
    }
    
    public async Task SetupContinuousComplianceMonitoringAsync()
    {
        // Surveillance en continu des contrôles critiques
        var criticalControls = new[]
        {
            "encryption_at_rest",
            "encryption_in_transit", 
            "access_controls",
            "audit_logging",
            "backup_integrity",
            "vulnerability_management"
        };
        
        foreach (var control in criticalControls)
        {
            await SetupControlMonitoringAsync(control);
        }
    }
}
```

Cette formation complète sur la sécurité cloud et infrastructure vous donne les outils et connaissances nécessaires pour sécuriser efficacement vos environnements cloud. Elle complète parfaitement les autres modules de votre formation en cybersécurité.

Voulez-vous que je crée d'autres cours spécialisés comme la sécurité mobile, l'analyse forensique, ou la réponse aux incidents ?
