﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SkEditor.API;
using SkEditor.Utilities.Parser;

namespace SkEditor.Parser.Elements;

public static class ElementParser
{

    public static void ParseNode(Node node, ParsingContext context)
    {
        if (node.Element != null) // Has been already parsed, maybe by an element.
            return;

        var registeredElements = Registries.ParserElements.ToList();
        registeredElements.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        registeredElements.Reverse();
        var elements = registeredElements.Select(x => x.Type).ToList();
        SkEditorAPI.Logs.Debug($"    - Parsing node '{node.Key}' with {elements.Count} elements.");
        
        foreach (var elementType in elements)
        {
            try
            {
                var method = elementType.GetMethod("Parse");
                if (method == null)
                {
                    SkEditorAPI.Logs.Warning($"Element class '{elementType.Name}' does not have a Parse method.");
                    continue;
                }

                if (Activator.CreateInstance(elementType) is not Element instance)
                {
                    SkEditorAPI.Logs.Warning($"Element class '{elementType.Name}' could not be instantiated.");
                    continue;
                }
                
                var result = method.Invoke(null, [node]) as bool?;
                if (result == true)
                {
                    try
                    {
                        instance.Load(node, context);
                    }
                    catch (ParsingException e)
                    {
                        context.Errors.Add((node, e.Message));
                        return;
                    }
                    
                    node.Element = instance;
                    context.ParsedNodes.Add(node);
                }
            }
            catch (Exception e)
            {
                SkEditorAPI.Logs.Fatal($"An error occurred while parsing node '{node.Key}' at line {node.Line} with element '{elementType.Name}': {e.Message}");
            }
        }
    }

    public static void ParseNodes(Node parent, ParsingContext context)
    {
        ParseNode(parent, context);
        if (parent is SectionNode section)
            foreach (var child in section.Children)
                ParseNodes(child, context);
    }
    
    public static void ParseNodes(IEnumerable<Node> nodes, ParsingContext context)
    {
        foreach (var node in nodes)
            ParseNodes(node, context);
    }
    
}