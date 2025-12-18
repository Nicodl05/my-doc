// Example Terraform manifest for Lab: create VPC + subnet + EC2 (AWS)
terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = { source = "hashicorp/aws" }
  }
}

provider "aws" {
  region = var.region
}

resource "aws_vpc" "main" {
  cidr_block = var.vpc_cidr
  tags = { Name = "tf-demo-vpc" }
}

resource "aws_subnet" "public" {
  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(var.vpc_cidr, 8, 0)
  availability_zone = var.az
  tags = { Name = "tf-demo-subnet" }
}

resource "aws_instance" "web" {
  ami           = var.ami
  instance_type = var.instance_type
  subnet_id     = aws_subnet.public.id
  tags = { Name = "tf-demo-web" }
}
