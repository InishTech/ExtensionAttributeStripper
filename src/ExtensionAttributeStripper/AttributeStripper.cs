using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace Stripper
{
    public class AttributeStripper
    {
        private readonly TypeDefinition _attributeToStrip;
        private readonly Func<CustomAttribute, bool> _attributeShouldBeStripped;

        public AttributeStripper( TypeDefinition attributeToStrip )
        {
            _attributeToStrip = attributeToStrip;
            _attributeShouldBeStripped = x => x.AttributeType == _attributeToStrip;
        }

        public void Process( AssemblyDefinition assembly )
        {
            StripFromMethods( assembly );

            StripFromTypes( assembly );

            StripFromAssembly( assembly );

            StripAttributeDefinition( assembly );
        }

        private void StripFromMethods( AssemblyDefinition assembly )
        {
            var methods = from mod in assembly.Modules
                          from type in mod.GetAllTypes()
                          from m in type.Methods
                          where m.CustomAttributes.Any( _attributeShouldBeStripped )
                          select m;

            foreach (var extensionMethod in methods)
                RemoveAttribute( extensionMethod.CustomAttributes );
        }

        private void StripFromTypes( AssemblyDefinition assembly )
        {
            var types = from mod in assembly.Modules
                        from type in mod.GetAllTypes()
                        where type.CustomAttributes.Any( _attributeShouldBeStripped )
                        select type;

            foreach (var extensionType in types)
                RemoveAttribute( extensionType.CustomAttributes );
        }

        private void StripFromAssembly( AssemblyDefinition assembly )
        {
            RemoveAttribute( assembly.CustomAttributes );
        }

        private void StripAttributeDefinition( AssemblyDefinition assembly )
        {
            assembly.MainModule.Types.Remove( _attributeToStrip );
        }

        private void RemoveAttribute( Collection<CustomAttribute> customAttributes )
        {
            var extattr = customAttributes.Single( x => x.AttributeType == _attributeToStrip );

            customAttributes.Remove( extattr );
        }
    }
}
