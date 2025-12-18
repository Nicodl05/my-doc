variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "az" {
  description = "Availability zone"
  type        = string
  default     = "us-east-1a"
}

variable "ami" {
  description = "AMI id"
  type        = string
  default     = "ami-0123456789abcdef0" # placeholder
}

variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t3.micro"
}
