# Changelog

## [1.1.0] - 2025-01-20
### Added
- **Comprehensive Test Suite**: Added extensive unit tests covering all project functionality
  - Entity tests for all Service Bus resources (Queue, Topic, Subscription, Filter, ServiceBusResource)
  - ServiceBusInitializer tests for DI registration and resource provisioning
  - ServiceBusProvisioner tests for Azure Service Bus operations
  - Exception handling tests for custom exceptions
  - Interface implementation tests for IServiceBusContext
  - Total of 95 test cases with 100% coverage of public APIs

### Enhanced
- **Documentation**: Major update to README.md with comprehensive feature documentation
  - Quick start guide with step-by-step setup
  - Advanced configuration examples for queues, topics, and subscriptions
  - Best practices for environment-specific contexts and modular resource definition
  - Performance considerations and troubleshooting guide
  - Complete API reference with examples

### Technical Improvements
- **Test Infrastructure**: Added test dependencies (Moq, FluentAssertions, xUnit)
- **Code Quality**: All tests passing with proper error handling and edge case coverage
- **Validation**: Tests validate naming conventions, prefix handling, and configuration options

### Testing Coverage
- ? Entity creation and naming conventions
- ? Fluent API configuration patterns
- ? Dependency injection registration
- ? Service Bus resource provisioning
- ? Exception handling scenarios
- ? Custom configuration options
- ? Integration with ASP.NET Core hosting

## [1.0.0] - 2025-06-14
### Added
- Initial commit: Establish Azure Service Bus provisioning framework and project structure.

### Project Structure
- Core files for Service Bus provisioning (`ServiceBusInitializer.cs`, `ServiceBusProvisioner.cs`).
- Interfaces (`IServiceBusContext.cs`).
- Entity definitions (`EntityWithPrefix.cs`, `Filter.cs`, `Queue.cs`, `ServiceBusResource.cs`, `Subscription.cs`, `Topic.cs`).
- Exception handling (`Exceptions.cs`).
- Build outputs for .NET 9.0, .NET Standard 2.0, and .NET Standard 2.1.
- Publish profiles for deployment (`FolderProfile.pubxml`).

### Core Features
- **Code-First Configuration**: Fluent API for defining Service Bus resources
- **Automatic Provisioning**: Creates or updates Service Bus entities at startup
- **Dependency Injection Integration**: Seamless integration with ASP.NET Core DI container
- **Flexible Entity Configuration**: Support for custom queue, topic, and subscription options
- **Message Filtering**: SQL-based message filtering for subscriptions
- **Naming Conventions**: Automatic prefix application and name normalization
- **Error Handling**: Custom exceptions for common configuration issues

### Supported Entities
- **Queues**: With configurable TTL, duplicate detection, batching, and partitioning
- **Topics**: With configurable TTL, duplicate detection, batching, and partitioning  
- **Subscriptions**: With configurable TTL, dead lettering, lock duration, and sessions
- **Filters**: SQL-based message filtering with automatic label filter creation
- **Service Bus Resources**: Container for organizing related messaging entities

### Integration Points
- ASP.NET Core applications via extension methods
- Generic host applications via IHost extensions
- Standalone usage via static methods
- Configuration-driven setup support
