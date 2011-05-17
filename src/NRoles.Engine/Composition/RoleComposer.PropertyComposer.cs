﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NRoles.Engine {

  partial class RoleComposer {

    sealed partial class MemberComposer {

      private static bool IsStateClassProperty(PropertyDefinition property) {
        return property.Name == NameProvider.GetStateClassPropertyName(property.DeclaringType.Name);
      }

      private PropertyDefinition RoleProperty { get { return (PropertyDefinition)_roleMember.Definition; } }
      private FieldDefinition _backingField;

      private PropertyDefinition ImplementPropertyCallingCodeClass() {
        var implementedProperty = ImplementProperty();
        var propertyAccessorComposer = new MemberComposer(TargetType, Container);
        ImplementPropertyAccessorMethods(implementedProperty, propertyAccessorComposer);
        return implementedProperty;
      }

      private PropertyDefinition ImplementPropertyWithBackingField() {
        var implementedProperty = ImplementProperty();
        var propertyAccessorComposer = new MemberComposer(TargetType, Container);
        propertyAccessorComposer._backingField = CreateBackingFieldFor(implementedProperty);
        ImplementPropertyAccessorMethods(implementedProperty, propertyAccessorComposer);
        return implementedProperty;
      }

      private void ImplementPropertyAccessorMethods(PropertyDefinition implementedProperty, MemberComposer propertyAccessorComposer) {
        if (RoleProperty.GetMethod != null) {
          var getterRoleGroup = Container.ResolveGroup(RoleProperty.GetMethod);
          implementedProperty.GetMethod = (MethodDefinition)propertyAccessorComposer.Compose(getterRoleGroup, _accessSpecifier);
        }
        if (RoleProperty.SetMethod != null) {
          var setterRoleGroup = Container.ResolveGroup(RoleProperty.SetMethod);
          implementedProperty.SetMethod = (MethodDefinition)propertyAccessorComposer.Compose(setterRoleGroup, _accessSpecifier);
        }
      }

      private void DontImplementPropertyAccessorMethods() {
        if (RoleProperty.GetMethod != null) {
          var getterRoleGroup = Container.ResolveGroup(RoleProperty.GetMethod);
          getterRoleGroup.DontImplement = true;
        }
        if (RoleProperty.SetMethod != null) {
          var setterRoleGroup = Container.ResolveGroup(RoleProperty.SetMethod);
          setterRoleGroup.DontImplement = true;
        }
      }

      private FieldDefinition CreateBackingFieldFor(PropertyDefinition implementedProperty) {
        var name = ResolveFieldName(implementedProperty);
        Tracer.TraceVerbose("Create backing field: {0}", name);
        var field = new FieldDefinition(
          name,
          // TODO?: FieldAttributes.Compilercontrolled |
          FieldAttributes.Private,
          implementedProperty.PropertyType);
        TargetType.Fields.Add(field);
        return field;
      }

      private string ResolveFieldName(PropertyDefinition implementedProperty) {
        return NameProvider.GetStateClassBackingFieldName(implementedProperty.Name);
      }

      private PropertyDefinition ImplementProperty() {
        Tracer.TraceVerbose("Compose property: {0}", _name);
        var implementedProperty = new MemberResolver(Role, Module).ResolvePropertyDefinition(RoleProperty);
        TargetType.Properties.Add(implementedProperty);
        return implementedProperty;
      }

    }

  }

}