# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Core implementation of the GitLab MCP Server.
- CI/CD pipelines including dependency management with Dependabot.
- Docker Hub automated build configuration.
- Automated release of pre-compiled binaries for Windows, macOS, and Linux.
- Open Source guidelines and issue templates.

### Changed
- Refactored project architecture to follow DDD (Domain-Driven Design).
- Centralized token management favoring `X-GitLab-Token` header for clients.
